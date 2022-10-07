// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Persian.Plus.PaymentGateway.Core.Storage.Abstractions.Models;

namespace Persian.Plus.PaymentGateway.Core.Internal
{
    public static class AdditionalDataConverter
    {
        public static IDictionary<string, string> ToDictionary(Transaction transaction)
        {
            return JsonConvert.DeserializeObject<IDictionary<string, string>>(transaction.AdditionalData);
        }

        public static string ToJson(PaymentResult paymentResult)
        {
            if (paymentResult == null) throw new ArgumentNullException(nameof(paymentResult));

            return JsonConvert.SerializeObject(paymentResult);
        }
    }
}
