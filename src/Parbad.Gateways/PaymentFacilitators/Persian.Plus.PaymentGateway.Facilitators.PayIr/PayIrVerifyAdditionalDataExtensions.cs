// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Persian.Plus.PaymentGateway.Core.Internal;

namespace Persian.Plus.PaymentGateway.Facilitators.PayIr
{
    public static class PayIrVerifyAdditionalDataExtensions
    {
        private const string AdditionalDataKey = "PayIrVerificationAdditionalData";

        public static PayIrVerifyAdditionalData GetPayIrAdditionalData(this PaymentVerifyResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));

            result.AdditionalData.TryGetValue(AdditionalDataKey, out var additionalData);

            return additionalData as PayIrVerifyAdditionalData;
        }

        internal static void SetPayIrAdditionalData(this PaymentVerifyResult result, PayIrVerifyAdditionalData additionalData)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            if (additionalData == null) throw new ArgumentNullException(nameof(additionalData));

            result.AdditionalData.Add(AdditionalDataKey, additionalData);
        }
    }
}
