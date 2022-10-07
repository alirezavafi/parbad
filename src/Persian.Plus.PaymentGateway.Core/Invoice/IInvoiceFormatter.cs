// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace Persian.Plus.PaymentGateway.Core.Invoice
{
    /// <summary>
    /// A formatter that can change an invoice.
    /// </summary>
    public interface IInvoiceFormatter
    {
        /// <summary>
        /// Formats the given <paramref name="invoice"/>.
        /// </summary>
        /// <param name="invoice"></param>
        /// <param name="cancellationToken"></param>
        Task FormatAsync(Gateway.Invoice invoice, CancellationToken cancellationToken = default);
    }
}
