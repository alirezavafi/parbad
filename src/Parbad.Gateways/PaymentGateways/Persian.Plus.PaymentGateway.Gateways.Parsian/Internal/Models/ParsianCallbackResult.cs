// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Persian.Plus.PaymentGateway.Gateways.Parsian.Internal.Models
{
    internal class ParsianCallbackResult
    {
        public bool IsSucceed { get; set; }

        public string Token { get; set; }

        /// <summary>
        /// Equals to TransactionCode in Persian.Plus.PaymentGateway.Core system.
        /// </summary>
        public string RRN { get; set; }

        public string Message { get; set; }
    }
}
