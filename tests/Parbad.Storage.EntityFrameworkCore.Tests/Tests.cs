using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Parbad.Storage.Abstractions.Models;
using Parbad.Storage.EntityFrameworkCore.Context;
using Parbad.Storage.EntityFrameworkCore.Options;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Parbad.Storage.EntityFrameworkCore.Tests
{
    public class Tests
    {
        private ServiceProvider _services;
        private EntityFrameworkCoreStorage _storage;

        private static Payment PaymentTestData => new Payment
        {
            TrackingNumber = 1,
            Amount = 1000,
            Token = "token",
            TransactionCode = "test",
            GatewayName = "gateway",
            GatewayAccountName = "test",
            IsPaid = false,
            IsCompleted = false
        };

        private static Transaction TransactionTestData => new Transaction
        {
            PaymentId = 1,
            Amount = 1000,
            IsSucceed = false,
            Message = "test",
            Type = TransactionType.Request,
            AdditionalData = "test"
        };

        [SetUp]
        public void Setup()
        {
            _services = new ServiceCollection()
                .Configure<EntityFrameworkCoreOptions>(options => { })
                .AddDbContext<ParbadDataContext>(builder => builder.UseInMemoryDatabase(Guid.NewGuid().ToString()))
                .BuildServiceProvider();

            var context = _services.GetRequiredService<ParbadDataContext>();

            _storage = new EntityFrameworkCoreStorage(context);
        }

        [TearDown]
        public ValueTask Cleanup()
        {
            return _services.DisposeAsync();
        }

        [Test]
        public async Task Create_Payment_Works()
        {
            await _storage.CreatePaymentAsync(PaymentTestData);

            var payment = await _storage.GetPaymentByTrackingNumberAsync(PaymentTestData.TrackingNumber);

            Assert.IsNotNull(payment);

            Assert.AreEqual(1, payment.Id);
            Assert.AreEqual(PaymentTestData.TrackingNumber, payment.TrackingNumber);
            Assert.AreEqual(PaymentTestData.Amount, payment.Amount);
            Assert.AreEqual(PaymentTestData.TransactionCode, payment.TransactionCode);
            Assert.AreEqual(PaymentTestData.GatewayName, payment.GatewayName);
            Assert.AreEqual(PaymentTestData.GatewayAccountName, payment.GatewayAccountName);
            Assert.AreEqual(PaymentTestData.Token, payment.Token);
            Assert.AreEqual(PaymentTestData.IsPaid, payment.IsPaid);
            Assert.AreEqual(PaymentTestData.IsCompleted, payment.IsCompleted);
        }

        [Test]
        public async Task Update_Payment_Works()
        {
            await _storage.CreatePaymentAsync(PaymentTestData);

            var payment = await _storage.GetPaymentByTrackingNumberAsync(PaymentTestData.TrackingNumber);
            payment.TrackingNumber = 2;
            payment.Amount = 2000;
            payment.Token = "NewToken";
            payment.TransactionCode = "NewCode";
            payment.GatewayName = "NewGateway";
            payment.GatewayAccountName = "NewAccount";
            payment.IsPaid = true;
            payment.IsCompleted = true;

            await _storage.UpdatePaymentAsync(payment);

            var newPayment = await _storage.GetPaymentByTrackingNumberAsync(payment.TrackingNumber);

            Assert.IsNotNull(newPayment);
            Assert.AreEqual(1, newPayment.Id);
            Assert.AreEqual(payment.TrackingNumber, newPayment.TrackingNumber);
            Assert.AreEqual(payment.Amount, newPayment.Amount);
            Assert.AreEqual(payment.TransactionCode, newPayment.TransactionCode);
            Assert.AreEqual(payment.GatewayName, newPayment.GatewayName);
            Assert.AreEqual(payment.GatewayAccountName, newPayment.GatewayAccountName);
            Assert.AreEqual(payment.Token, newPayment.Token);
            Assert.AreEqual(payment.IsPaid, newPayment.IsPaid);
            Assert.AreEqual(payment.IsCompleted, newPayment.IsCompleted);
        }

        [Test]
        public async Task Create_Transaction_Works()
        {
            await _storage.CreatePaymentAsync(PaymentTestData);

            var payment = await _storage.GetPaymentByTrackingNumberAsync(PaymentTestData.TrackingNumber);

            TransactionTestData.PaymentId = payment.Id;

            await _storage.CreateTransactionAsync(TransactionTestData);

            var transaction = (await _storage.GetTransactionsAsync(payment.Id)).FirstOrDefault(x => x.PaymentId == payment.Id);

            Assert.IsNotNull(transaction);

            Assert.AreEqual(1, transaction.Id);
            Assert.AreEqual(payment.Id, transaction.PaymentId);
            Assert.AreEqual(TransactionTestData.Amount, transaction.Amount);
            Assert.AreEqual(TransactionTestData.AdditionalData, transaction.AdditionalData);
            Assert.AreEqual(TransactionTestData.IsSucceed, transaction.IsSucceed);
            Assert.AreEqual(TransactionTestData.Type, transaction.Type);
            Assert.AreEqual(TransactionTestData.Message, transaction.Message);
        }
    }
}
