// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Persian.Plus.PaymentGateway.Core.Invoice;

namespace Persian.Plus.PaymentGateway.Facilitators.PayIr
{
    public static class PayIrGatewayInvoiceBuilderExtensions
    {
        /// <summary>
        /// The invoice will be sent to Pay.ir gateway.
        /// </summary>
        /// <param name="builder"></param>
        public static IInvoiceBuilder UsePayIr(this IInvoiceBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.SetGateway(PayIrGateway.Name);
        }
    }
}
