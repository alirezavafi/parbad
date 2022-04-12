// Copyright (c) Parbad.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Parbad.Abstraction;
using Parbad.Gateway.Saman.Internal.Models;
using Parbad.Gateway.Saman.Internal.ResultTranslators;
using Parbad.Http;
using Parbad.Internal;
using Parbad.Net;
using Parbad.Options;
using Parbad.Storage.Abstractions.Models;
using Parbad.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Parbad.Gateway.Saman.Internal
{
    internal static class SamanHelper
    {
        public const string AdditionalVerificationDataKey = "SamanAdditionalVerificationData";

        public static Task<PaymentRequestResult> CreateRequest(
            Invoice invoice,
            HttpContext httpContext,
            SamanGatewayAccount account,
            HttpClient httpClient,
            SamanGatewayOptions gatewayOptions,
            MessagesOptions messagesOptions,
            CancellationToken cancellationToken)
        {
            return CreatePaymentRequest(invoice, httpContext, account, httpClient, gatewayOptions, messagesOptions, cancellationToken);
        }

        public static async Task<SamanCallbackResult> CreateCallbackResultAsync(
            HttpRequest httpRequest,
            MessagesOptions messagesOptions,
            CancellationToken cancellationToken)
        {
            var isSuccess = false;
            string message = null;
            StringValues referenceId = "";
            StringValues transactionId = "";

            var securePan = await httpRequest.TryGetParamAsync("SecurePan", cancellationToken).ConfigureAwaitFalse();
            var cid = await httpRequest.TryGetParamAsync("CID", cancellationToken).ConfigureAwaitFalse();
            var traceNo = await httpRequest.TryGetParamAsync("TraceNo", cancellationToken).ConfigureAwaitFalse();
            var rrn = await httpRequest.TryGetParamAsync("RRN", cancellationToken).ConfigureAwaitFalse();

            var state = await httpRequest.TryGetParamAsync("state", cancellationToken).ConfigureAwaitFalse();

            if (!state.Exists || state.Value.IsNullOrEmpty())
            {
                message = messagesOptions.InvalidDataReceivedFromGateway;
            }
            else
            {
                var referenceIdResult = await httpRequest.TryGetParamAsync("ResNum", cancellationToken).ConfigureAwaitFalse();
                if (referenceIdResult.Exists) referenceId = referenceIdResult.Value;

                var transactionIdResult = await httpRequest.TryGetParamAsync("RefNum", cancellationToken).ConfigureAwaitFalse();
                if (transactionIdResult.Exists) transactionId = transactionIdResult.Value;

                isSuccess = state.Value.Equals("OK", StringComparison.OrdinalIgnoreCase);

                if (!isSuccess)
                {
                    message = SamanStateTranslator.Translate(state.Value, messagesOptions);
                }
            }

            return new SamanCallbackResult
            {
                IsSucceed = isSuccess,
                ReferenceId = referenceId,
                TransactionId = transactionId,
                SecurePan = securePan.Value,
                Cid = cid.Value,
                TraceNo = traceNo.Value,
                Rrn = rrn.Value,
                Message = message,
                State = state.ToString(),
            };
        }

        public static PaymentVerifyResult CreateVerifyResult(SamanVerifyTransactionResult verifyResult, InvoiceContext context, SamanCallbackResult callbackResult, MessagesOptions messagesOptions)
        {
            var isSuccess = verifyResult.Success && verifyResult.ResultCode == 0 && verifyResult.SamanVerifyTransactionDetail.AffectiveAmount == (long)context.Payment.Amount;

            var message = isSuccess
                ? messagesOptions.PaymentSucceed
                : verifyResult.ResultDescription;

            var result = new PaymentVerifyResult
            {
                Status = isSuccess ? PaymentVerifyResultStatus.Succeed : PaymentVerifyResultStatus.Failed,
                TransactionCode = callbackResult.TransactionId,
                Message = message
            };

            result.AdditionalData.Add(AdditionalVerificationDataKey, new SamanAdditionalVerificationData
            {
                Cid = callbackResult.Cid,
                TraceNo = callbackResult.TraceNo,
                SecurePan = callbackResult.SecurePan,
                Rrn = callbackResult.Rrn
            });

            return result;
        }

        public static string CreateRefundData(InvoiceContext context, Money amount, SamanGatewayAccount account)
        {
            return
                "<soapenv:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:urn=\"urn:Foo\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<urn:reverseTransaction soapenv:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">" +
                $"<String_1 xsi:type=\"xsd:string\">{context.Payment.TransactionCode}</String_1>" +
                $"<String_2 xsi:type=\"xsd:string\">{(long)amount}</String_2>" +
                $"<Username xsi:type=\"xsd:string\">{account.MerchantId}</Username>" +
                $"<Password xsi:type=\"xsd:string\">{account.Password}</Password>" +
                "</urn:reverseTransaction>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }

        public static PaymentRefundResult CreateRefundResult(string webServiceResponse, MessagesOptions messagesOptions)
        {
            var result = XmlHelper.GetNodeValueFromXml(webServiceResponse, "result");

            var integerResult = Convert.ToInt32(result);

            var isSucceed = integerResult > 0;

            var message = SamanResultTranslator.Translate(integerResult, messagesOptions);

            return new PaymentRefundResult
            {
                Status = isSucceed ? PaymentRefundResultStatus.Succeed : PaymentRefundResultStatus.Failed,
                Message = message
            };
        }
        
        private static async Task<PaymentRequestResult> CreatePaymentRequest(
            Invoice invoice,
            HttpContext httpContext,
            SamanGatewayAccount account,
            HttpClient httpClient,
            SamanGatewayOptions gatewayOptions,
            MessagesOptions messagesOptions,
            CancellationToken cancellationToken)
        {
            var data = new SamanPaymentTokenRequest
            {
                TerminalId = account.MerchantId,
                ResNum = invoice.TrackingNumber.ToString(),
                Amount = invoice.Amount,
                RedirectUrl = invoice.CallbackUrl,
                CellNumber = invoice.MobileNumber,
                Action = "token"
            };

            var responseMessage = await httpClient.PostJsonAsync(gatewayOptions.TokenUrl, data, cancellationToken);

            var response = await responseMessage.Content.ReadAsStringAsync();

            var tokenResponse = JsonConvert.DeserializeObject<SamanPaymentTokenResponse>(response);

            if (tokenResponse == null)
            {
                var message = $"{messagesOptions.InvalidDataReceivedFromGateway} Serialized token response is null.";
                return PaymentRequestResult.Failed(message, account.Name);
            }

            if (tokenResponse.Status == -1)
            {
                return PaymentRequestResult.Failed(tokenResponse.GetError(), account.Name);
            }

            var result = PaymentRequestResult.SucceedWithPost(
                account.Name,
                gatewayOptions.PaymentPageUrl,
                new Dictionary<string, string>
                {
                    {"Token", tokenResponse.Token}
                },
                tokenResponse);

            return result;
        }
    }
}
