// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Persian.Plus.PaymentGateway.Core.Exceptions;
using Persian.Plus.PaymentGateway.Core.Internal;
using Persian.Plus.PaymentGateway.Core.PaymentTokenProviders;

namespace Persian.Plus.PaymentGateway.Core
{
    /// <summary>
    /// Provides an easy solution to perform payment request, verify the requested payment and
    /// refund a payment.
    /// </summary>
    public interface IOnlinePayment
    {
        /// <summary>
        /// Defines a mechanism for retrieving a service object; that is, an object that
        /// provides custom support to other objects.
        /// </summary>
        IServiceProvider Services { get; }

        /// <summary>
        /// Performs a payment request using the given <paramref name="invoice"/>.
        /// </summary>
        /// <param name="invoice">The invoice that must be paid.</param>
        /// <param name="cancellationToken"></param>
        Task<PaymentRequestResult> RequestAsync(Gateway.Invoice invoice, CancellationToken cancellationToken = default);

        /// <summary>
        /// Fetches the invoice from the incoming HTTP request.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <exception cref="InvalidPaymentTokenProviderException"></exception>
        /// <exception cref="InvoiceNotFoundException"></exception>
        Task<PaymentFetchResult> FetchAndStoreAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Fetches the invoice by the given tracking number.
        /// </summary>
        /// <param name="trackingNumber">Invoice's tracking number.</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="InvalidPaymentTokenProviderException"></exception>
        /// <exception cref="InvoiceNotFoundException"></exception>
        Task<PaymentFetchResult> FetchAsync(long trackingNumber, CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifies the requested payment to check whether or not the invoice was paid in the gateway by the client.
        /// This method must be called when the fetch result equals to <see cref="PaymentFetchResultStatus.ReadyForVerifying"/>.
        /// </summary>
        /// <param name="trackingNumber">The tracking number of the invoice which must be verified.</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="InvoiceNotFoundException"></exception>
        Task<PaymentVerifyResult> VerifyAsync(long trackingNumber, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cancels the given invoice. No Verifying request will be sent to the gateway.
        /// </summary>
        /// <param name="trackingNumber">The tracking number of the invoice which must be verified.</param>
        /// <param name="cancellationReason">The reason for canceling the operation. It will be saved in Message field in database.</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="InvoiceNotFoundException"></exception>
        Task<PaymentCancelResult> CancelAsync(long trackingNumber, string cancellationReason = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Performs a refund request for the given invoice.
        /// </summary>
        /// <param name="invoice">The invoice that must be refunded.</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="InvoiceNotFoundException"></exception>
        Task<PaymentRefundResult> RefundAsync(RefundInvoice invoice, CancellationToken cancellationToken = default);
    }
}
