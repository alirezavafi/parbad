// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Persian.Plus.PaymentGateway.Core.Exceptions;
using Persian.Plus.PaymentGateway.Core.Gateway;
using Persian.Plus.PaymentGateway.Core.Options;
using Persian.Plus.PaymentGateway.Core.PaymentTokenProviders;
using Persian.Plus.PaymentGateway.Core.Storage.Abstractions;
using Persian.Plus.PaymentGateway.Core.Storage.Abstractions.Models;
using Serilog;
using Serilog.Context;

namespace Persian.Plus.PaymentGateway.Core.Internal
{
    /// <inheritdoc />
    public class DefaultOnlinePayment : IOnlinePayment
    {
        private readonly IPaymentStorage _paymentStorage;
        private readonly IPaymentTokenProvider _tokenProvider;
        private readonly IGatewayProvider _gatewayProvider;
        private readonly ILogger _logger;
        private readonly ParbadOptions _options;

        /// <summary>
        /// Initializes an instance of <see cref="DefaultOnlinePayment"/>.
        /// </summary>
        public DefaultOnlinePayment(
            IServiceProvider services,
            IPaymentStorage paymentStorage,
            IPaymentTokenProvider tokenProvider,
            IGatewayProvider gatewayProvider,
            IOptions<ParbadOptions> options,
            ILogger logger)
        {
            Services = services;
            _paymentStorage = paymentStorage;
            _tokenProvider = tokenProvider;
            _options = options.Value;
            _paymentStorage = paymentStorage;
            _gatewayProvider = gatewayProvider;
            _logger = logger;
        }

        /// <inheritdoc />
        public IServiceProvider Services { get; }

        /// <inheritdoc />
        public virtual async Task<PaymentRequestResult> RequestAsync(Gateway.Invoice invoice, CancellationToken cancellationToken = default)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));
            using var l1 = LogContext.PushProperty("TrackingNumber", invoice.TrackingNumber);
            using var l2 = LogContext.PushProperty("MobileNumber", invoice.MobileNumber);
            
            _logger.Information("Request {@Invoice} started", invoice);

            //  Check the tracking number
            var existingPayment = await _paymentStorage.GetPaymentByTrackingNumberAsync(invoice.TrackingNumber, cancellationToken);
            if (existingPayment != null)
            {
                if (existingPayment.IsCompleted)
                {
                    _logger.Warning("{@Invoice} already processed with {@Payment}", invoice, existingPayment);

                    return new PaymentRequestResult
                    {
                        TrackingNumber = invoice.TrackingNumber,
                        Status = PaymentRequestResultStatus.TrackingNumberAlreadyExists,
                        Message = _options.Messages.DuplicateTrackingNumber,
                    };
                }
                else
                {
                    var transactions = await _paymentStorage.GetTransactionsAsync(existingPayment.Id, cancellationToken);
                    var requestTransaction = transactions.FirstOrDefault(x => x.Type == TransactionType.Request);
                    if (requestTransaction == null)
                    {
                        throw new InvalidOperationException("payment request transaction not found!");
                    }
                    else
                    {
                        var res = JsonConvert.DeserializeObject<PaymentRequestResult>(requestTransaction.AdditionalData);
                        _logger.Information("Continuing processing {@Invoice} with {@Result}", invoice, res);
                        return res;
                    }
                }
            }

            // Create a payment token
            var paymentToken = await _tokenProvider
                .ProvideTokenAsync(invoice, cancellationToken)
                .ConfigureAwaitFalse();

            if (paymentToken.IsNullOrEmpty())
                throw new InvalidPaymentTokenProviderException(paymentToken, "EmptyToken");

            if (await _paymentStorage.DoesPaymentExistAsync(paymentToken, cancellationToken).ConfigureAwaitFalse())
                throw new InvalidPaymentTokenProviderException(paymentToken, "DuplicatePaymentToken");

            var gateway = _gatewayProvider.Provide(invoice.GatewayName);

            var newPayment = new Payment
            {
                TrackingNumber = invoice.TrackingNumber,
                Amount = invoice.Amount,
                IsCompleted = false,
                IsPaid = false,
                Token = paymentToken,
                GatewayName = gateway.GetRoutingGatewayName()
            };

            await _paymentStorage.CreatePaymentAsync(newPayment, cancellationToken).ConfigureAwaitFalse();

            PaymentRequestResult requestResult = null;

            try
            {
                requestResult = await gateway
                    .RequestAsync(invoice, cancellationToken)
                    .ConfigureAwaitFalse() as PaymentRequestResult;

                if (requestResult == null) throw new InvalidOperationException("Empty result");
            }
            catch (Exception exception)
            {
                newPayment.IsCompleted = true;
                newPayment.IsPaid = false;
                _logger.Error(exception, "Failed to request {@Payment} with {@Result}", newPayment, requestResult);
                requestResult = PaymentRequestResult.Failed(exception.Message);
            }

            requestResult.TrackingNumber = invoice.TrackingNumber;
            requestResult.Amount = invoice.Amount;
            requestResult.GatewayName = gateway.GetRoutingGatewayName();

            newPayment.GatewayAccountName = requestResult.GatewayAccountName;

            var newTransaction = new Transaction
            {
                Amount = invoice.Amount,
                Type = TransactionType.Request,
                IsSucceed = requestResult.IsSucceed,
                Message = requestResult.Message,
                AdditionalData = AdditionalDataConverter.ToJson(requestResult),
                PaymentId = newPayment.Id
            };

            _logger.Information("{@Payment}, {@Transaction} created successfully with {@Result}", newPayment, newTransaction, requestResult);

            await _paymentStorage.UpdatePaymentAsync(newPayment, cancellationToken).ConfigureAwaitFalse();
            await _paymentStorage.CreateTransactionAsync(newTransaction, cancellationToken).ConfigureAwaitFalse();

            return requestResult;
        }

        /// <inheritdoc />
        public virtual async Task<PaymentFetchResult> FetchAndStoreAsync(CancellationToken cancellationToken = default)
        {
            var paymentToken = await _tokenProvider.RetrieveTokenAsync(cancellationToken).ConfigureAwaitFalse();

            if (string.IsNullOrEmpty(paymentToken))
                throw new InvalidPaymentTokenProviderException(paymentToken, "No Token is received.");

            var payment = await _paymentStorage.GetPaymentByLocalTokenAsync(paymentToken, cancellationToken).ConfigureAwaitFalse();

            if (payment == null)
                throw new InvoiceNotFoundException(paymentToken);

            var result = (PaymentFetchResult)await FetchAsync(payment, cancellationToken);
            var transaction = new Transaction
            {
                Amount = result.Amount,
                IsSucceed = result.IsSucceed,
                Message = result.Message,
                Type = TransactionType.Callback,
                AdditionalData = JsonConvert.SerializeObject(result.CallbackResult),
                PaymentId = payment.Id
            };

            var tr = (await _paymentStorage.GetTransactionsAsync(payment.Id, cancellationToken)).FirstOrDefault(x => x.Type == TransactionType.Callback);
            if (tr == null)
                await _paymentStorage.CreateTransactionAsync(transaction, cancellationToken).ConfigureAwaitFalse();

            return result;
        }

        /// <inheritdoc />
        public async Task<PaymentFetchResult> FetchAsync(long trackingNumber, CancellationToken cancellationToken = default)
        {
            var payment = await _paymentStorage.GetPaymentByTrackingNumberAsync(trackingNumber, cancellationToken).ConfigureAwaitFalse();
            if (payment == null)
                throw new InvoiceNotFoundException(trackingNumber);
            
            var result = await FetchAsync(payment, cancellationToken);
            
            return result;
        }

        /// <inheritdoc />
        public virtual async Task<PaymentVerifyResult> VerifyAsync(long trackingNumber, CancellationToken cancellationToken = default)
        {
            using var l1 = LogContext.PushProperty("TrackingNumber", trackingNumber);
            var payment = await _paymentStorage
                .GetPaymentByTrackingNumberAsync(trackingNumber, cancellationToken)
                .ConfigureAwaitFalse();

            if (payment == null)
                throw new InvoiceNotFoundException(trackingNumber);

            if (payment.IsCompleted)
            {
                var paymentVerifyResult = new PaymentVerifyResult
                {
                    TrackingNumber = payment.TrackingNumber,
                    Amount = payment.Amount,
                    GatewayName = payment.GatewayName,
                    GatewayAccountName = payment.GatewayAccountName,
                    TransactionCode = payment.TransactionCode,
                    Status = payment.IsPaid ? PaymentVerifyResultStatus.AlreadyVerified : PaymentVerifyResultStatus.Failed,
                    Message = _options.Messages.PaymentIsAlreadyProcessedBefore
                };

                _logger.Information("{@Payment} already processed with {@Result}", payment, paymentVerifyResult);
                
                return paymentVerifyResult;
            }

            var gateway = _gatewayProvider.Provide(payment.GatewayName);

            var transactions = await _paymentStorage.GetTransactionsAsync(payment.Id, cancellationToken).ConfigureAwaitFalse();
            var invoiceContext = new InvoiceContext(payment, transactions);

            PaymentVerifyResult verifyResult = null;

            verifyResult = await gateway
                .VerifyAsync(invoiceContext, cancellationToken)
                .ConfigureAwaitFalse() as PaymentVerifyResult;

            if (verifyResult == null)
                throw new InvalidOperationException("Verification result is null!!!");

            verifyResult.TrackingNumber = payment.TrackingNumber;
            verifyResult.Amount = payment.Amount;
            verifyResult.GatewayName = payment.GatewayName;
            verifyResult.GatewayAccountName = payment.GatewayAccountName;

            payment.IsCompleted = true;
            payment.IsPaid = verifyResult.IsSucceed;
            payment.TransactionCode = verifyResult.TransactionCode;

            var transaction = new Transaction
            {
                Amount = verifyResult.Amount,
                IsSucceed = verifyResult.IsSucceed,
                Message = verifyResult.Message,
                Type = TransactionType.Verify,
                AdditionalData = AdditionalDataConverter.ToJson(verifyResult),
                PaymentId = payment.Id
            };

            _logger.Information("{@Payment} verified with {@Transaction} {@Result}", payment, transaction, verifyResult);

            await _paymentStorage.UpdatePaymentAsync(payment, cancellationToken).ConfigureAwaitFalse();
            await _paymentStorage.CreateTransactionAsync(transaction, cancellationToken).ConfigureAwaitFalse();

            return verifyResult;
        }

        /// <inheritdoc />
        public virtual async Task<PaymentCancelResult> CancelAsync(long trackingNumber, string cancellationReason = null, CancellationToken cancellationToken = default)
        {
            using var l1 = LogContext.PushProperty("TrackingNumber", trackingNumber);

            var payment = await _paymentStorage
                .GetPaymentByTrackingNumberAsync(trackingNumber, cancellationToken)
                .ConfigureAwaitFalse();

            if (payment == null)
                throw new InvoiceNotFoundException(trackingNumber);

            if (payment.IsCompleted)
            {
                var paymentCancelResult = new PaymentCancelResult
                {
                    TrackingNumber = payment.TrackingNumber,
                    Amount = payment.Amount,
                    GatewayName = payment.GatewayName,
                    GatewayAccountName = payment.GatewayAccountName,
                    IsSucceed = false,
                    Message = _options.Messages.PaymentIsAlreadyProcessedBefore
                };

                _logger.Information("{@Payment} cancelled with {@Result}", payment, paymentCancelResult);
                return paymentCancelResult;
            }

            var message = cancellationReason ?? _options.Messages.PaymentCanceledProgrammatically;

            payment.IsCompleted = true;
            payment.IsPaid = false;

            var newTransaction = new Transaction
            {
                Amount = payment.Amount,
                IsSucceed = false,
                Message = message,
                Type = TransactionType.Canceled,
                PaymentId = payment.Id
            };

            var cancelResult = new PaymentCancelResult
            {
                TrackingNumber = payment.TrackingNumber,
                Amount = payment.Amount,
                IsSucceed = true,
                GatewayName = payment.GatewayName,
                GatewayAccountName = payment.GatewayAccountName,
                Message = message
            };
            
            _logger.Information("{@Payment} cancel finished with {@Transaction} {@Result}", payment, newTransaction, cancelResult);

            await _paymentStorage.UpdatePaymentAsync(payment, cancellationToken).ConfigureAwaitFalse();
            await _paymentStorage.CreateTransactionAsync(newTransaction, cancellationToken).ConfigureAwaitFalse();
            
            return cancelResult;
        }

        /// <inheritdoc />
        public virtual async Task<PaymentRefundResult> RefundAsync(RefundInvoice invoice, CancellationToken cancellationToken = default)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            using var l1 = LogContext.PushProperty("TrackingNumber", invoice.TrackingNumber);

            var payment = await _paymentStorage.GetPaymentByTrackingNumberAsync(invoice.TrackingNumber, cancellationToken).ConfigureAwaitFalse();

            if (payment == null)
                throw new InvoiceNotFoundException(invoice.TrackingNumber);

            if (!payment.IsCompleted)
            {
                _logger.Warning("{@Payment} not completed to do refund", payment);
                return PaymentRefundResult.Failed("PaymentNotCompleted");
            }

            Money amountToRefund;

            if (invoice.Amount == 0)
            {
                amountToRefund = payment.Amount;
            }
            else if (invoice.Amount > payment.Amount)
            {
                _logger.Warning("{@Invoice} amount greater than {@Payment} amount", invoice, payment);
                throw new InvalidOperationException("Amount cannot be greater than the amount of the paid payment.");
            }
            else
            {
                amountToRefund = invoice.Amount;
            }

            var gateway = _gatewayProvider.Provide(payment.GatewayName);

            var transactions = await _paymentStorage.GetTransactionsAsync(payment.Id, cancellationToken).ConfigureAwaitFalse();
            var verifyContext = new InvoiceContext(payment, transactions);

            PaymentRefundResult refundResult = null;

            refundResult = await gateway
                .RefundAsync(verifyContext, amountToRefund, cancellationToken)
                .ConfigureAwaitFalse() as PaymentRefundResult;

            if (refundResult == null)
                throw new InvalidOperationException("Refund result is null");

            refundResult.TrackingNumber = payment.TrackingNumber;
            refundResult.Amount = amountToRefund;
            refundResult.GatewayName = payment.GatewayName;
            refundResult.GatewayAccountName = payment.GatewayAccountName;

            var newtTransaction = new Transaction
            {
                Amount = refundResult.Amount,
                Type = TransactionType.Refund,
                IsSucceed = refundResult.IsSucceed,
                Message = refundResult.Message,
                AdditionalData = AdditionalDataConverter.ToJson(refundResult),
                PaymentId = payment.Id
            };

            _logger.Information("{@Refund}, {@Transaction} perfomed with {@Result}", new { RefundAmount = amountToRefund}, newtTransaction, refundResult);

            await _paymentStorage.CreateTransactionAsync(newtTransaction, cancellationToken).ConfigureAwaitFalse();

            return refundResult;
        }

        private async Task<PaymentFetchResult> FetchAsync(Payment payment, CancellationToken cancellationToken)
        {
            var fetchResult = new PaymentFetchResult
            {
                TrackingNumber = payment.TrackingNumber,
                Amount = payment.Amount,
                GatewayName = payment.GatewayName,
                GatewayAccountName = payment.GatewayAccountName,
                IsAlreadyVerified = payment.IsPaid
            };

            if (payment.IsCompleted)
            {
                fetchResult.Status = PaymentFetchResultStatus.AlreadyProcessed;
                fetchResult.Message = _options.Messages.PaymentIsAlreadyProcessedBefore;

                return fetchResult;
            }

            var gateway = _gatewayProvider.Provide(payment.GatewayName);

            var transactions = await _paymentStorage.GetTransactionsAsync(payment.Id, cancellationToken).ConfigureAwaitFalse();
            var invoiceContext = new InvoiceContext(payment, transactions);

            PaymentFetchResult gatewayFetchResult;

            gatewayFetchResult = await gateway
                .FetchAsync(invoiceContext, cancellationToken)
                .ConfigureAwaitFalse() as PaymentFetchResult;

            if (gatewayFetchResult == null) 
                throw new InvalidOperationException($"Null result");

            fetchResult.Status = gatewayFetchResult.Status;
            fetchResult.CallbackResult = gatewayFetchResult.CallbackResult;
            
            string message = null;
            if (gatewayFetchResult.Status != PaymentFetchResultStatus.ReadyForVerifying)
            {
                message = gatewayFetchResult.Message ?? _options.Messages.PaymentFailed;
            }
            fetchResult.Message = message;
            return fetchResult;
        }
    }
}
