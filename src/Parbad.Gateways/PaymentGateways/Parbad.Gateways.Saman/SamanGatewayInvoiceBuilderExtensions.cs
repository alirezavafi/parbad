// Copyright (c) Parbad.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Abstraction;
using Parbad.Gateway.Saman.Internal;
using Parbad.InvoiceBuilder;
using System;

namespace Parbad.Gateway.Saman
{
    public static class SamanGatewayInvoiceBuilderExtensions
    {
        /// <summary>
        /// The invoice will be sent to Saman gateway.
        /// </summary>
        /// <param name="builder"></param>
        public static IInvoiceBuilder UseSaman(this IInvoiceBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.SetGateway(SamanGateway.Name);
        }
    }
}
