﻿// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Persian.Plus.PaymentGateway.Facilitators.PayPing.Internal
{
    internal class PayPingCallbackResult
    {
        public bool IsSucceed { get; set; }

        public string Message { get; set; }

        public string RefId { get; set; }
    }
}
