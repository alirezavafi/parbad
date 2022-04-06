﻿// Copyright (c) Parbad.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;

namespace Parbad.Gateway.PayIr.Internal
{
    internal class PayIrRequestResponseModel
    {
        public string Token { get; set; }

        public string Status { get; set; }

        public string ErrorCode { get; set; }

        public string ErrorMessage { get; set; }

        public bool IsSucceed => string.Equals(Status, "1", StringComparison.InvariantCultureIgnoreCase);
    }
}
