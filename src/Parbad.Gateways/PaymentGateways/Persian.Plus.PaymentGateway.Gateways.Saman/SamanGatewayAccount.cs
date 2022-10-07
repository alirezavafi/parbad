// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Persian.Plus.PaymentGateway.Core.Gateway;

namespace Persian.Plus.PaymentGateway.Gateways.Saman
{
    public class SamanGatewayAccount : GatewayAccount
    {
        public string MerchantId { get; set; }

        public string Password { get; set; }
    }
}
