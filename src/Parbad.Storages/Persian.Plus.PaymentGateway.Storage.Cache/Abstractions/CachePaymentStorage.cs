﻿// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Persian.Plus.PaymentGateway.Core.Storage.Abstractions;
using Persian.Plus.PaymentGateway.Core.Storage.Abstractions.Models;

namespace Persian.Plus.PaymentGateway.Storage.Cache.Abstractions
{
    /// <summary>
    /// Abstract cache implementation of Persian.Plus.PaymentGateway.Core storage.
    /// </summary>
    public abstract class CachePaymentStorage : IPaymentStorage
    {
        /// <summary>
        /// A collection for holding the data.
        /// </summary>
        protected abstract ICacheStorageCollection Collection { get; }

        /// <inheritdoc />
        public virtual Task CreatePaymentAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            if (payment == null) throw new ArgumentNullException(nameof(payment));
            cancellationToken.ThrowIfCancellationRequested();

            payment.Id = GenerateNewPaymentId();

            var record = FindPayment(payment);
            if (record != null) throw new InvalidOperationException($"There is already a payment record in database with id {payment.Id}");

            Collection.Payments.Add(payment);

            return SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc />
        public virtual Task UpdatePaymentAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            if (payment == null) throw new ArgumentNullException(nameof(payment));
            cancellationToken.ThrowIfCancellationRequested();

            var record = FindPayment(payment);

            if (record == null) throw new InvalidOperationException($"No payment records found in database with id {payment.Id}");

            record.Token = payment.Token;
            record.TrackingNumber = payment.TrackingNumber;
            record.TransactionCode = payment.TransactionCode;

            return SaveChangesAsync(cancellationToken);
        }
        
        /// <inheritdoc />
        public virtual Task CreateTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));
            cancellationToken.ThrowIfCancellationRequested();

            transaction.Id = GenerateNewTransactionId();

            var record = FindTransaction(transaction);
            if (record != null) throw new InvalidOperationException($"There is already a transaction record in database with id {transaction.Id}");

            Collection.Transactions.Add(transaction);

            return SaveChangesAsync(cancellationToken);
        }
        
        /// <inheritdoc />
        public virtual Task<Payment> GetPaymentByTrackingNumberAsync(long trackingNumber, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(Collection.Payments.SingleOrDefault(model => model.TrackingNumber == trackingNumber));
        }

        public Task<Payment> GetPaymentByLocalTokenAsync(string paymentToken, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(Collection.Payments.SingleOrDefault(model => model.Token == paymentToken));
        }

        /// <inheritdoc />
        public virtual Task<Payment> GetPaymentByTokenAsync(string paymentToken, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(Collection.Payments.SingleOrDefault(model => model.Token == paymentToken));
        }

        /// <inheritdoc />
        public virtual Task<bool> DoesPaymentExistAsync(long trackingNumber, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(Collection.Payments.Any(model => model.TrackingNumber == trackingNumber));
        }

        /// <inheritdoc />
        public virtual Task<bool> DoesPaymentExistAsync(string paymentToken, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Collection.Payments.Any(model => model.Token == paymentToken));
        }

        /// <inheritdoc />
        public virtual Task<List<Transaction>> GetTransactionsAsync(long paymentId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Collection.Transactions.Where(model => model.PaymentId == paymentId).ToList());
        }

        /// <summary>   
        /// Finds a payment in storage.
        /// </summary>
        /// <param name="payment"></param>
        protected virtual Payment FindPayment(Payment payment)
        {
            return Collection.Payments.Contains(payment)
                ? payment
                : Collection.Payments.SingleOrDefault(model => model.Id == payment.Id);
        }

        /// <summary>
        /// Finds a transaction in storage.
        /// </summary>
        /// <param name="transaction"></param>
        protected virtual Transaction FindTransaction(Transaction transaction)
        {
            return Collection.Transactions.Contains(transaction)
                ? transaction
                : Collection.Transactions.SingleOrDefault(model => model.Id == transaction.Id);
        }

        /// <summary>
        /// Generates a unique id for a new payment record.
        /// </summary>
        protected virtual long GenerateNewPaymentId()
        {
            return Collection.Payments.Count == 0
                ? 1
                : Collection.Payments.Max(model => model.Id) + 1;
        }

        /// <summary>
        /// Generates a unique id for a new transaction record.
        /// </summary>
        protected virtual long GenerateNewTransactionId()
        {
            return Collection.Transactions.Count == 0
                ? 1
                : Collection.Transactions.Max(model => model.Id) + 1;
        }

        /// <summary>
        /// Saves the current data in storage.
        /// </summary>
        /// <param name="cancellationToken"></param>
        protected abstract Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
