﻿// Copyright (c) Parbad.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace Parbad.Gateway.IdPay.Internal
{
    internal class IdPayVerifyModel
    {
        public string Id { get; set; }

        [JsonProperty("order_id")]
        public long OrderId { get; set; }
    }
}