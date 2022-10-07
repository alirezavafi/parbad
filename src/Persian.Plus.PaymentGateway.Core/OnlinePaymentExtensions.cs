// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Persian.Plus.PaymentGateway.Core.Exceptions;
using Persian.Plus.PaymentGateway.Core.Internal;
using Persian.Plus.PaymentGateway.Core.Invoice;
using Persian.Plus.PaymentGateway.Core.PaymentTokenProviders;

namespace Persian.Plus.PaymentGateway.Core
{
    public static class OnlinePaymentExtensions
    {
        /// <summary>
        /// Performs a new payment request with the given data.
        /// </summary>
        /// <param name="onlinePayment"></param>
        /// <param name="gatewayName">The gateway which the client must pay the invoice in.</param>
        /// <param name="trackingNumber">
        /// A tracking number for this request. It will be sent to the gateway.
        /// <para>Note: It must be unique for each requests.</para>
        /// </param>
        /// <param name="amount">The amount of the payment request.</param>
        /// <param name="callbackUrl">
        /// A complete URL of your website. It will be used by the gateway for redirecting the client again to your website.
        /// <para>A complete URL would be like: "http://www.mywebsite.com/foo/bar/"</para>
        /// </param>
        public static PaymentRequestResult Request(
            this IOnlinePayment onlinePayment,
            string gatewayName,
            long trackingNumber,
            decimal amount,
            string callbackUrl) =>
            onlinePayment.RequestAsync(gatewayName, trackingNumber, amount, callbackUrl)
                .GetAwaiter()
                .GetResult();

        /// <summary>
        /// Performs a new payment request with the given data.
        /// </summary>
        /// <param name="onlinePayment"></param>
        /// <param name="configureInvoice">A builder which helps to build an invoice.</param>
        public static PaymentRequestResult Request(this IOnlinePayment onlinePayment, Action<IInvoiceBuilder> configureInvoice)
            => onlinePayment.RequestAsync(configureInvoice)
                .GetAwaiter()
                .GetResult();

        /// <summary>
        /// Performs a new payment request with the given invoice.
        /// </summary>
        /// <param name="onlinePayment"></param>
        /// <param name="invoice">The invoice that must be paid.</param>
        public static PaymentRequestResult Request(this IOnlinePayment onlinePayment, Gateway.Invoice invoice) =>
            onlinePayment.RequestAsync(invoice)
                .GetAwaiter()
                .GetResult();

        /// <summary>
        /// Performs a new payment request with the given data.
        /// </summary>
        /// <param name="onlinePayment"></param>
        /// <param name="gatewayName">The gateway which the client must pay the invoice in.</param>
        /// <param name="trackingNumber">
        /// A tracking number for this request. It will be sent to the gateway.
        /// <para>Note: It must be unique for each requests.</para>
        /// </param>
        /// <param name="amount">The amount of the payment request.</param>
        /// <param name="callbackUrl">
        /// A complete URL of your website. It will be used by the gateway for redirecting the client again to your website.
        /// <para>A complete URL would be like: "http://www.mywebsite.com/foo/bar/"</para>
        /// </param>
        /// <param name="cancellationToken"></param>
        public static Task<PaymentRequestResult> RequestAsync(
            this IOnlinePayment onlinePayment,
            string gatewayName,
            long trackingNumber,
            decimal amount,
            string callbackUrl,
            CancellationToken cancellationToken = default)
        {
            return onlinePayment.RequestAsync(builder =>
            {
                builder
                    .SetTrackingNumber(trackingNumber)
                    .SetAmount(amount)
                    .SetCallbackUrl(callbackUrl)
                    .SetGateway(gatewayName);
            }, cancellationToken);
        }

        /// <summary>
        /// Performs a new payment request by using an invoice builder.
        /// </summary>
        /// <param name="onlinePayment"></param>
        /// <param name="configureInvoice">A builder which helps to build an invoice.</param>
        /// <param name="cancellationToken"></param>
        public static async Task<PaymentRequestResult> RequestAsync(
            this IOnlinePayment onlinePayment,
            Action<IInvoiceBuilder> configureInvoice,
            CancellationToken cancellationToken = default)
        {
            if (onlinePayment == null) throw new ArgumentNullException(nameof(onlinePayment));
            if (configureInvoice == null) throw new ArgumentNullException(nameof(configureInvoice));

            IInvoiceBuilder invoiceBuilder = new DefaultInvoiceBuilder(onlinePayment.Services);

            configureInvoice(invoiceBuilder);

            var invoice = await invoiceBuilder.BuildAsync(cancellationToken).ConfigureAwaitFalse();

            return await onlinePayment.RequestAsync(invoice, cancellationToken);
        }

        /// <summary>
        /// Fetches the invoice from the incoming request.
        /// </summary>
        /// <param name="onlinePayment"></param>
        /// <exception cref="InvalidPaymentTokenProviderException"></exception>
        /// <exception cref="InvoiceNotFoundException"></exception>
        public static PaymentFetchResult Fetch(this IOnlinePayment onlinePayment)
            => onlinePayment.FetchAndStoreAsync().GetAwaiter().GetResult();

        /// <summary>
        /// Fetches the invoice by the given tracking number.
        /// </summary>
        /// <param name="onlinePayment"></param>
        /// <param name="trackingNumber">Invoice's tracking number.</param>
        /// <exception cref="InvalidPaymentTokenProviderException"></exception>
        /// <exception cref="InvoiceNotFoundException"></exception>
        public static PaymentFetchResult Fetch(this IOnlinePayment onlinePayment, long trackingNumber)
            => onlinePayment.FetchAsync(trackingNumber).GetAwaiter().GetResult();

        /// <summary>
        /// Verifies the given invoice.
        /// </summary>
        /// <param name="onlinePayment"></param>
        /// <param name="trackingNumber">The tracking number of the invoice which must be verified.</param>
        /// <exception cref="InvoiceNotFoundException"></exception>
        public static PaymentVerifyResult Verify(this IOnlinePayment onlinePayment, long trackingNumber)
            => onlinePayment.VerifyAsync(trackingNumber).GetAwaiter().GetResult();

        /// <summary>
        /// Verifies the given invoice.
        /// </summary>
        /// <param name="onlinePayment"></param>
        /// <param name="invoice"></param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="InvoiceNotFoundException"></exception>
        public static Task<PaymentVerifyResult> VerifyAsync(
            this IOnlinePayment onlinePayment,
            PaymentFetchResult invoice,
            CancellationToken cancellationToken = default)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            return onlinePayment.VerifyAsync(invoice.TrackingNumber, cancellationToken);
        }

        /// <summary>
        /// Verifies the given invoice.
        /// </summary>
        /// <param name="onlinePayment"></param>
        /// <param name="invoice"></param>
        /// <exception cref="InvoiceNotFoundException"></exception>
        public static PaymentVerifyResult Verify(this IOnlinePayment onlinePayment, PaymentFetchResult invoice)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            return onlinePayment.VerifyAsync(invoice.TrackingNumber).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Cancels the given invoice. No Verifying request will be sent to the gateway.
        /// </summary>
        /// <param name="onlinePayment"></param>
        /// <param name="trackingNumber">The tracking number of the invoice which must be verified.</param>
        /// <param name="cancellationReason">The reason for canceling the operation. It will be saved in Message field in database.</param>
        /// <exception cref="InvoiceNotFoundException"></exception>
        public static PaymentCancelResult Cancel(
            this IOnlinePayment onlinePayment,
            long trackingNumber,
            string cancellationReason = null)
            => onlinePayment.CancelAsync(trackingNumber, cancellationReason).GetAwaiter().GetResult();

        /// <summary>
        /// Cancels the given invoice. No Verifying request will be sent to the gateway.
        /// </summary>
        /// <param name="onlinePayment"></param>
        /// <param name="invoice"></param>
        /// <param name="cancellationReason">The reason for canceling the operation. It will be saved in Message field in database.</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="InvoiceNotFoundException"></exception>
        public static Task<PaymentCancelResult> CancelAsync(
            this IOnlinePayment onlinePayment,
            PaymentFetchResult invoice,
            string cancellationReason = null,
            CancellationToken cancellationToken = default)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            return onlinePayment.CancelAsync(invoice.TrackingNumber, cancellationReason, cancellationToken);
        }

        /// <summary>
        /// Cancels the given invoice. No Verifying request will be sent to the gateway.
        /// </summary>
        /// <param name="onlinePayment"></param>
        /// <param name="invoice"></param>
        /// <param name="cancellationReason">The reason for canceling the operation. It will be saved in Message field in database.</param>
        /// <exception cref="InvoiceNotFoundException"></exception>
        public static PaymentCancelResult Cancel(
            this IOnlinePayment onlinePayment,
            PaymentFetchResult invoice,
            string cancellationReason = null)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            return onlinePayment.CancelAsync(invoice.TrackingNumber, cancellationReason).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Performs a refund request for the given invoice.
        /// </summary>
        /// <param name="onlinePayment"></param>
        /// <param name="invoice">The invoice that must be refunded.</param>
        public static PaymentRefundResult Refund(this IOnlinePayment onlinePayment, RefundInvoice invoice)
            => onlinePayment.RefundAsync(invoice)
                .GetAwaiter()
                .GetResult();

        /// <summary>
        /// Refunds completely a specific payment with the given tracking number.
        /// </summary>
        /// <param name="onlinePayment"></param>
        /// <param name="trackingNumber">The tracking number of the payment that must be refunded.</param>
        public static PaymentRefundResult RefundCompletely(this IOnlinePayment onlinePayment, long trackingNumber) =>
            onlinePayment.RefundAsync(new RefundInvoice(trackingNumber))
                .GetAwaiter()
                .GetResult();

        /// <summary>
        /// Refunds completely the paid payment.
        /// </summary>
        /// <param name="onlinePayment"></param>
        /// <param name="verifyResult"></param>
        public static PaymentRefundResult RefundCompletely(this IOnlinePayment onlinePayment, PaymentVerifyResult verifyResult)
        {
            if (onlinePayment == null) throw new ArgumentNullException(nameof(onlinePayment));
            if (verifyResult == null) throw new ArgumentNullException(nameof(verifyResult));

            return onlinePayment.Refund(new RefundInvoice(verifyResult.TrackingNumber));
        }

        /// <summary>
        /// Refunds a specific amount of a  with the given tracking number.
        /// <para>Note: Only Saman Gateway supports this operation.</para>
        /// </summary>
        /// <param name="onlinePayment"></param>
        /// <param name="trackingNumber">The tracking number of the payment which must be refunded.</param>
        /// <param name="amount">Amount of refund.</param>
        public static PaymentRefundResult RefundSpecificAmount(
            this IOnlinePayment onlinePayment,
            long trackingNumber,
            decimal amount) =>
            onlinePayment.RefundSpecificAmountAsync(trackingNumber, amount)
                .GetAwaiter()
                .GetResult();

        /// <summary>
        /// Refunds completely a specific payment with the given tracking number.
        /// </summary>
        /// <param name="onlinePayment"></param>
        /// <param name="trackingNumber">The tracking number of the payment that must be refunded.</param>
        /// <param name="cancellationToken"></param>
        public static Task<PaymentRefundResult> RefundCompletelyAsync(
            this IOnlinePayment onlinePayment,
            long trackingNumber,
            CancellationToken cancellationToken = default) =>
            onlinePayment.RefundAsync(new RefundInvoice(trackingNumber), cancellationToken);

        /// <summary>
        /// Refunds a specific amount of a payment with the given tracking number.
        /// <para>Note: Only Saman Gateway supports this operation.</para>
        /// </summary>
        /// <param name="onlinePayment"></param>
        /// <param name="trackingNumber">The tracking number of the payment that must be refunded.</param>
        /// <param name="amount">Amount of refund.</param>
        /// <param name="cancellationToken"></param>
        public static Task<PaymentRefundResult> RefundSpecificAmountAsync(
            this IOnlinePayment onlinePayment,
            long trackingNumber,
            decimal amount,
            CancellationToken cancellationToken = default) =>
            onlinePayment.RefundAsync(new RefundInvoice(trackingNumber, amount), cancellationToken);

        /// <summary>
        /// Refunds completely the paid payment.
        /// </summary>
        /// <param name="onlinePayment"></param>
        /// <param name="verifyResult"></param>
        /// <param name="cancellationToken"></param>
        public static Task<PaymentRefundResult> RefundCompletelyAsync(
            this IOnlinePayment onlinePayment,
            PaymentVerifyResult verifyResult,
            CancellationToken cancellationToken = default)
        {
            if (onlinePayment == null) throw new ArgumentNullException(nameof(onlinePayment));
            if (verifyResult == null) throw new ArgumentNullException(nameof(verifyResult));

            return onlinePayment.RefundAsync(new RefundInvoice(verifyResult.TrackingNumber), cancellationToken);
        }
    }
}
