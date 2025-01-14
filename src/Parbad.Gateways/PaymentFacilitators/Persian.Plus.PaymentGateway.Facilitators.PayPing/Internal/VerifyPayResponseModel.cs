﻿// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Persian.Plus.PaymentGateway.Facilitators.PayPing.Internal
{
    internal class VerifyPayResponseModel
    {
        public long Amount { get; set; }

        public string CardNumber { get; set; }

        public string CardHashPan { get; set; }
    }
}
