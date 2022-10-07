// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Persian.Plus.PaymentGateway.Core.Internal;

namespace Persian.Plus.PaymentGateway.Core
{
    public static class GatewayTransporterExtensions
    {
        /// <summary>
        /// Transports the client to the specified gateway.
        /// </summary>
        public static void Transport(this IGatewayTransporter transporter)
        {
            if (transporter == null) throw new ArgumentNullException(nameof(transporter));

            transporter.TransportAsync()
                .ConfigureAwaitFalse()
                .GetAwaiter()
                .GetResult();
        }
    }
}
