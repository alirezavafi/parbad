// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Persian.Plus.PaymentGateway.Core.Gateway;

namespace Persian.Plus.PaymentGateway.Facilitators.IdPay
{
    public class IdPayGatewayAccount : GatewayAccount
    {
        public string Api { get; set; }

        public bool IsTestAccount { get; set; }
    }
}
