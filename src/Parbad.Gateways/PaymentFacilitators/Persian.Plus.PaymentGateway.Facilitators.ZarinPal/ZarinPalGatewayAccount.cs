﻿// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Persian.Plus.PaymentGateway.Core.Gateway;

namespace Persian.Plus.PaymentGateway.Facilitators.ZarinPal
{
    /// <summary>
    /// Describes an account for ZarinPal Gateway.
    /// </summary>
    public class ZarinPalGatewayAccount : GatewayAccount
    {
        public string MerchantId { get; set; }

        public bool IsSandbox { get; set; }
    }
}
