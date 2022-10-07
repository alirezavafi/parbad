// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Persian.Plus.PaymentGateway.Gateways.Saman.Internal.Models
{
    public class SamanPaymentTokenRequest
    {
        public string Action { get; set; }

        public string TerminalId { get; set; }

        public string RedirectUrl { get; set; }

        public string ResNum { get; set; }

        public long Amount { get; set; }

        public string CellNumber { get; set; }
    }
}
