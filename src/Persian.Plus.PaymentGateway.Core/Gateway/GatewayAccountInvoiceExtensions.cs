// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Persian.Plus.PaymentGateway.Core.Internal;
using Persian.Plus.PaymentGateway.Core.Invoice;

namespace Persian.Plus.PaymentGateway.Core.Gateway
{
    public static class GatewayAccountInvoiceExtensions
    {
        /// <summary>
        /// Gateway Account key in <see cref="Invoice.Properties"/> property.
        /// </summary>
        public const string GatewayAccountKeyName = "AccountName";

        /// <summary>
        /// Uses the given account to communicate with the gateway.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="accountName">Name of the account.</param>
        public static IInvoiceBuilder UseAccount(this IInvoiceBuilder builder, string accountName)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (accountName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(accountName));

            builder.AddProperty(GatewayAccountKeyName, accountName);

            return builder;
        }

        /// <summary>
        /// Gets the account name if specified.
        /// </summary>
        /// <param name="invoice"></param>
        public static string GetAccountName(this Invoice invoice)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            if (!invoice.Properties.ContainsKey(GatewayAccountKeyName)) return null;

            return (string)invoice.Properties[GatewayAccountKeyName];
        }
    }
}
