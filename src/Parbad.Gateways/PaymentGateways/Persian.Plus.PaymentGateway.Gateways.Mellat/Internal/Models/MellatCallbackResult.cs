// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Persian.Plus.PaymentGateway.Gateways.Mellat.Internal.Models
{
    internal class MellatCallbackResult
    {
        public bool IsSucceed { get; set; }

        public string RefId { get; set; }

        public string SaleReferenceId { get; set; }

        public string Message { get; set; }
        public string CardNo { get; set; }
    }
}
