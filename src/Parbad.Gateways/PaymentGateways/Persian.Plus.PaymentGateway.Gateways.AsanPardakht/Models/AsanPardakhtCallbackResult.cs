// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Persian.Plus.PaymentGateway.Gateways.AsanPardakht.Models
{
    public  class AsanPardakhtCallbackResult
    {
        public bool IsSucceed { get; set; }

        public string PayGateTranId { get; set; }

        public string Rrn { get; set; }

        public string CardNumber { get; set; }

        public string Message { get; set; }
        public string Status { get; set; }
    }
}
