// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Persian.Plus.PaymentGateway.Core.Internal
{
    public class PaymentFetchResult : PaymentResult
    {
        public PaymentFetchResultStatus Status { get; set; }

        public bool IsAlreadyVerified { get; set; }
        public object CallbackResult { get; set; }

        public override bool IsSucceed => Status == PaymentFetchResultStatus.ReadyForVerifying;

        public static PaymentFetchResult Failed(object callbackResult, string message)
        {
            return new PaymentFetchResult
            {
                Status = PaymentFetchResultStatus.Failed,
                Message = message,
                CallbackResult = callbackResult
            };
        }

        public static PaymentFetchResult ReadyForVerifying(object callbackResult)
        {
            return new PaymentFetchResult
            {
                Status = PaymentFetchResultStatus.ReadyForVerifying,
                CallbackResult = callbackResult
            };
        }
    }
}
