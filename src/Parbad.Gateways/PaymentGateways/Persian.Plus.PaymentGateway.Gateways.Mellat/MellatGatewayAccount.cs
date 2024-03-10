// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Persian.Plus.PaymentGateway.Core.Gateway;

namespace Persian.Plus.PaymentGateway.Gateways.Mellat
{
    public class MellatGatewayAccount : GatewayAccount
    {
        public long TerminalId { get; set; }

        public string UserName { get; set; }

        public string UserPassword { get; set; }
        public string PinBlockKeyHex { get; set; }
        public string PinBlockVectorHex { get; set; }
    }
}
