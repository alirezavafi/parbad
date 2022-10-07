// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Persian.Plus.PaymentGateway.Gateways.Saman
{
    public class SamanGatewayOptions
    {
        public string PaymentPageUrl { get; set; } = "https://sep.shaparak.ir/OnlinePG/OnlinePG";
        public string TokenUrl { get; set; } = "https://sep.shaparak.ir/onlinepg/onlinepg";
        public string VerificationUrl { get; set; } = "https://sep.shaparak.ir/verifyTxnRandomSessionkey/ipg/VerifyTransaction";
    }
}
