// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Persian.Plus.PaymentGateway.Gateways.Saman.Internal.Models
{
    internal class SamanPaymentTokenResponse
    {
        public int Status { get; set; }

        public string ErrorCode { get; set; }

        public string ErrorDesc { get; set; }

        public string Token { get; set; }

        public string GetError()
        {
            return $"{nameof(ErrorCode)}: {ErrorCode}, {nameof(ErrorDesc)}: {ErrorDesc}";
        }
    }
}
