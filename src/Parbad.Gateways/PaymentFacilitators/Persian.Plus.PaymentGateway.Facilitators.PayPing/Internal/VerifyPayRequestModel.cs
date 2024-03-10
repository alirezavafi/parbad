﻿// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Persian.Plus.PaymentGateway.Facilitators.PayPing.Internal
{
    public class VerifyPayRequestModel
    {
        public long Amount { get; set; }

        public string RefId { get; set; }
    }
}