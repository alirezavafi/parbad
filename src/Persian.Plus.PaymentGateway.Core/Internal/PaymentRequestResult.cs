// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Persian.Plus.PaymentGateway.Core.Internal
{
    public class PaymentRequestResult : PaymentResult
    {
        /// <inheritdoc />
        public PaymentRequestResultStatus Status { get; set; }

        public IGatewayTransporter GetGatewayTransporter(HttpContext httpContext) {
            if (IsSucceed)
            {
                if (IsFormPostMethod)
                {
                    var descriptor = GatewayTransporterDescriptor.CreatePost(PaymentPageUrl, FormData);
                    var transporter = new DefaultGatewayTransporter(httpContext, descriptor);
                    return transporter;
                }
                else 
                {
                    var descriptor = GatewayTransporterDescriptor.CreateRedirect(PaymentPageUrl);
                    var transporter = new DefaultGatewayTransporter(httpContext, descriptor);
                    return transporter;
                }
            }
            else
            {
                return new NullGatewayTransporter();
            }
        }

        public object GatewayResult { get; set; }

        [JsonIgnore]
        public override bool IsSucceed => Status == PaymentRequestResultStatus.Succeed;

        public Dictionary<string, string> FormData { get; set; } = new();

       
        public static PaymentRequestResult SucceedWithPost(
            string gatewayAccountName,
            string url,
            IEnumerable<KeyValuePair<string, string>> form,
            object gatewayResult)
        {
            var paymentRequestResult = Succeed(gatewayAccountName, gatewayResult);
            paymentRequestResult.IsFormPostMethod = true;
            paymentRequestResult.FormData = new Dictionary<string, string>(form);
            paymentRequestResult.PaymentPageUrl = url;
            return paymentRequestResult;
        }

        public bool IsFormPostMethod { get; set; }
        public string PaymentPageUrl { get; set; }

        public static PaymentRequestResult SucceedWithRedirect(
            string gatewayAccountName,
            string url,
            object gatewayResult)
        {
            var paymentRequestResult = Succeed(gatewayAccountName, gatewayResult);
            paymentRequestResult.IsFormPostMethod = false;
            paymentRequestResult.PaymentPageUrl = url;
            return paymentRequestResult;
        }

        public static PaymentRequestResult Succeed(string gatewayAccountName, object gatewayResult, Dictionary<string, string> formData = null)
        {
            return new PaymentRequestResult
            {
                GatewayAccountName = gatewayAccountName,
                //GatewayTransporter = gatewayTransporter,
                Status = PaymentRequestResultStatus.Succeed,
                GatewayResult = gatewayResult,
                FormData = formData,
                IsFormPostMethod = true,
            };
        }

        public static PaymentRequestResult Failed(string message, string gatewayAccountName = null, object gatewayResult = null)
        {
            return new PaymentRequestResult
            {
                Status = PaymentRequestResultStatus.Failed,
                Message = message,
                GatewayAccountName = gatewayAccountName,
                // GatewayTransporter = new NullGatewayTransporter(),
                GatewayResult = gatewayResult,
            };
        }
    }
}
