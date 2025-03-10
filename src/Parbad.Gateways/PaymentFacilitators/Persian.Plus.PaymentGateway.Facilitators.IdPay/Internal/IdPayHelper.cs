﻿// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Persian.Plus.PaymentGateway.Core;
using Persian.Plus.PaymentGateway.Core.Gateway;
using Persian.Plus.PaymentGateway.Core.Http;
using Persian.Plus.PaymentGateway.Core.Internal;
using Persian.Plus.PaymentGateway.Core.Net;
using Persian.Plus.PaymentGateway.Core.Options;

namespace Persian.Plus.PaymentGateway.Facilitators.IdPay.Internal
{
    internal static class IdPayHelper
    {
        public const string ApiKey = "X-API-KEY";
        public const string SandBoxKey = "X-SANDBOX";
        public const string SandBoxApiValue = "6a7f99eb-7c20-4412-a972-6dfb7cd253a4";
        public const string Succeed = "100";
        public const string ReadyForVerifying = "10";

        public static object CreateRequestData(Invoice invoice)
        {
            return new IdPayRequestModel
            {
                OrderId = invoice.TrackingNumber,
                Amount = invoice.Amount,
                Callback = invoice.CallbackUrl
            };
        }

        public static async Task<PaymentRequestResult> CreateRequestResult(
            HttpResponseMessage responseMessage,
            HttpContext httpContext,
            IdPayGatewayAccount account)
        {
            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            if (!responseMessage.IsSuccessStatusCode)
            {
                var errorModel = JsonConvert.DeserializeObject<IdPayErrorModel>(response);

                return PaymentRequestResult.Failed(errorModel.ToString(), account.Name);
            }

            var result = JsonConvert.DeserializeObject<IdPayRequestResultModel>(response);

            return PaymentRequestResult.SucceedWithRedirect(account.Name, result.Link, result);
        }

        public static async Task<IdPayCallbackResult> CreateCallbackResultAsync(
            InvoiceContext context,
            HttpRequest httpRequest,
            MessagesOptions messagesOptions,
            CancellationToken cancellationToken)
        {
            var status = await httpRequest.TryGetParamAsync("status", cancellationToken).ConfigureAwaitFalse();
            var id = await httpRequest.TryGetParamAsync("id", cancellationToken).ConfigureAwaitFalse();
            var trackId = await httpRequest.TryGetParamAsync("track_id", cancellationToken).ConfigureAwaitFalse();
            var orderId = await httpRequest.TryGetParamAsync("order_id", cancellationToken).ConfigureAwaitFalse();
            var amount = await httpRequest.TryGetParamAsync("amount", cancellationToken).ConfigureAwaitFalse();

            var (isSucceed, message) = CheckCallback(status.Value, orderId.Value, id.Value, trackId.Value, amount.Value, context, messagesOptions);

            return new IdPayCallbackResult
            {
                Id = id.Value,
                IsSucceed = isSucceed,
                Message = message
            };
        }

        public static IdPayVerifyModel CreateVerifyData(InvoiceContext context, IdPayCallbackResult callbackResult)
        {
            return new IdPayVerifyModel
            {
                Id = callbackResult.Id,
                OrderId = context.Payment.TrackingNumber
            };
        }

        public static async Task<PaymentVerifyResult> CreateVerifyResult(HttpResponseMessage responseMessage, MessagesOptions messagesOptions)
        {
            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            if (!responseMessage.IsSuccessStatusCode)
            {
                var errorModel = JsonConvert.DeserializeObject<IdPayErrorModel>(response);

                return PaymentVerifyResult.Failed(errorModel.ToString());
            }

            var result = JsonConvert.DeserializeObject<IdPayVerifyResultModel>(response);

            if (result == null)
            {
                return PaymentVerifyResult.Failed(messagesOptions.InvalidDataReceivedFromGateway);
            }

            if (result.Status.IsNullOrEmpty() || result.TrackId.IsNullOrEmpty())
            {
                return PaymentVerifyResult.Failed(messagesOptions.InvalidDataReceivedFromGateway);
            }

            if (result.Status != Succeed)
            {
                return PaymentVerifyResult.Failed($"Verification failed. Status: {result.Status}");
            }

            return PaymentVerifyResult.Succeed(result.TrackId, messagesOptions.PaymentSucceed);
        }

        public static void AssignHeaders(HttpRequestHeaders headers, IdPayGatewayAccount account)
        {
            var api = account.IsTestAccount ? SandBoxApiValue : account.Api;

            headers.AddOrUpdate(ApiKey, api);

            if (account.IsTestAccount)
            {
                headers.AddOrUpdate(SandBoxKey, "1");
            }
            else if (headers.Contains(SandBoxKey))
            {
                headers.Remove(SandBoxKey);
            }
        }

        private static (bool IsSucceed, string Message) CheckCallback(
            string status,
            string orderId,
            string id,
            string trackId,
            string amount,
            InvoiceContext context,
            MessagesOptions messagesOptions)
        {
            if (status.IsNullOrEmpty())
            {
                return (false, messagesOptions.InvalidDataReceivedFromGateway);
            }

            if (status != ReadyForVerifying)
            {
                return (false, IdPayResultTranslator.TranslateStatus(status, messagesOptions));
            }

            if (orderId.IsNullOrEmpty() ||
                id.IsNullOrEmpty() ||
                trackId.IsNullOrEmpty() ||
                amount.IsNullOrEmpty())
            {
                return (false, messagesOptions.InvalidDataReceivedFromGateway);
            }

            if (!long.TryParse(orderId, out var integerOrderId))
            {
                return (false, messagesOptions.InvalidDataReceivedFromGateway);
            }

            if (integerOrderId != context.Payment.TrackingNumber)
            {
                return (false, messagesOptions.InvalidDataReceivedFromGateway);
            }

            if (!long.TryParse(amount, out var integerAmount))
            {
                return (false, messagesOptions.InvalidDataReceivedFromGateway);
            }

            if (integerAmount != context.Payment.Amount)
            {
                return (false, messagesOptions.InvalidDataReceivedFromGateway);
            }

            return (true, null);
        }
    }
}
