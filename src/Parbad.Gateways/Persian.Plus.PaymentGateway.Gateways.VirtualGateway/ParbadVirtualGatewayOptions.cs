// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;

namespace Persian.Plus.PaymentGateway.Gateways.VirtualGateway
{
    public class ParbadVirtualGatewayOptions
    {
        /// <summary>
        /// Path of Virtual Gateway. It would be like: /MyParbadGateway
        /// </summary>
        public PathString GatewayPath { get; set; }
    }
}
