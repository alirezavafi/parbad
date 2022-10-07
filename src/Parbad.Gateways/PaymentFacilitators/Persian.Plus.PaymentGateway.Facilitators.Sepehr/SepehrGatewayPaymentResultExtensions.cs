// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Persian.Plus.PaymentGateway.Core.Internal;
using Persian.Plus.PaymentGateway.Facilitators.Sepehr.Internal;

namespace Persian.Plus.PaymentGateway.Facilitators.Sepehr
{
    public static class SepehrGatewayPaymentResultExtensions
    {
        /// <summary>
        /// Gets the verification additional data from Sepehr gateway.
        /// </summary>
        public static SepehrGatewayVerificationAdditionalData GetSepehrAdditionalData(this PaymentVerifyResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));

            result.AdditionalData.TryGetValue(SepehrHelper.VerificationAdditionalDataKey, out var additionalData);

            return additionalData as SepehrGatewayVerificationAdditionalData;
        }

        internal static void SetSepehrAdditionalData(this PaymentVerifyResult result, SepehrGatewayVerificationAdditionalData additionalData)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            if (additionalData == null) throw new ArgumentNullException(nameof(additionalData));

            result.AdditionalData.Add(SepehrHelper.VerificationAdditionalDataKey, additionalData);
        }
    }
}
