using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Parbad.Storage.Abstractions;
using Parbad.Storage.Abstractions.Models;
using Parbad.Storage.EntityFrameworkCore.Context;
using Parbad.Storage.EntityFrameworkCore.Internal;

namespace Parbad.Storage.EntityFrameworkCore
{
    /// <summary>
    /// EntityFramework Core implementation of <see cref="IPaymentStorage"/>.
    /// </summary>
    public class EntityFrameworkCorePaymentStorage : IPaymentStorage
    {
        protected ParbadDataContext DbContext { get; }

        public EntityFrameworkCorePaymentStorage(ParbadDataContext dbContext)
        {
            DbContext = dbContext;
        }

        /// <inheritdoc />
        public async Task CreatePaymentAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            if (payment == null) throw new ArgumentNullException(nameof(payment));

            var entity = payment.ToEntity();
            entity.CreatedOn = DateTime.UtcNow;

            DbContext.Payments.Add(entity);

            await DbContext.SaveChangesAsync(cancellationToken);

            DbContext.Entry(entity).State = EntityState.Detached;

            payment.Id = entity.Id;
        }

        /// <inheritdoc />
        public async Task UpdatePaymentAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            if (payment == null) throw new ArgumentNullException(nameof(payment));

            var record = await DbContext
                .Payments
                .AsNoTracking()
                .SingleOrDefaultAsync(model => model.Id == payment.Id, cancellationToken);

            if (record == null) throw new InvalidOperationException($"No payment records found in database with id {payment.Id}");

            Mapper.ToEntity(payment, record);
            record.UpdatedOn = DateTime.UtcNow;

            DbContext.Payments.Update(record);

            await DbContext.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task CreateTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));

            var entity = transaction.ToEntity();
            entity.CreatedOn = DateTime.UtcNow;
            switch (entity.Type)
            {
                case TransactionType.Request:
                    entity.TypeCode = "REQUEST";
                    entity.TypeTitle = "درخواست پرداخت";
                    break;
                case TransactionType.Callback:
                    entity.TypeCode = "PSP_CALLBACK";
                    entity.TypeTitle = "بازگشت از درگاه";
                    break;
                case TransactionType.Verify:
                    entity.TypeCode = "VERIFY";
                    entity.TypeTitle = "تاییده پرداخت";
                    break;
                case TransactionType.Canceled:
                    entity.TypeCode = "VERIFY_CANCEL";
                    entity.TypeTitle = "لغو پرداخت";
                    break;
                case TransactionType.Refund:
                    entity.TypeCode = "REFUND";
                    entity.TypeTitle = "استرداد وجه";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            DbContext.Transactions.Add(entity);

            await DbContext.SaveChangesAsync(cancellationToken);

            transaction.Id = entity.Id;
        }

        /// <inheritdoc />
        public Task<Payment> GetPaymentByTrackingNumberAsync(long trackingNumber, CancellationToken cancellationToken = default)
        {
            var p = DbContext.Payments
                .AsNoTracking()
                .SingleOrDefault(payment => payment.TrackingNumber == trackingNumber);
            return Task.FromResult(p?.ToModel());
        }

        /// <inheritdoc />
        public Task<Payment> GetPaymentByLocalTokenAsync(string paymentToken, CancellationToken cancellationToken = default)
        {
            var p = DbContext.Payments
                .AsNoTracking()
                .SingleOrDefault(payment => payment.Token == paymentToken);
            return Task.FromResult(p?.ToModel());
        }

        /// <inheritdoc />
        public Task<bool> DoesPaymentExistAsync(long trackingNumber, CancellationToken cancellationToken = default)
        {
            var result = DbContext.Payments.Any(payment => payment.TrackingNumber == trackingNumber);
            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public Task<bool> DoesPaymentExistAsync(string paymentToken, CancellationToken cancellationToken = default)
        {
            var result = DbContext.Payments.Any(payment => payment.Token == paymentToken);
            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public Task<List<Transaction>> GetTransactionsAsync(long paymentId, CancellationToken cancellationToken = default)
        {
            var result = DbContext.Transactions
                .Where(transaction => transaction.PaymentId == paymentId)
                .AsNoTracking()
                .ToList();
            return Task.FromResult(result.Select(x => x.ToModel()).ToList());
        }
    }
}
