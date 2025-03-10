// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Persian.Plus.PaymentGateway.Core.Internal
{
    public class PaymentRefundResult : PaymentResult
    {
        public PaymentRefundResultStatus Status { get; set; }

        public override bool IsSucceed => Status == PaymentRefundResultStatus.Succeed;

        public static PaymentRefundResult Succeed(string message = null)
        {
            return new PaymentRefundResult
            {
                Status = PaymentRefundResultStatus.Succeed,
                Message = message ?? string.Empty
            };
        }

        public static PaymentRefundResult Failed(string message)
        {
            return new PaymentRefundResult
            {
                Status = PaymentRefundResultStatus.Failed,
                Message = message ?? string.Empty
            };
        }
    }
}
