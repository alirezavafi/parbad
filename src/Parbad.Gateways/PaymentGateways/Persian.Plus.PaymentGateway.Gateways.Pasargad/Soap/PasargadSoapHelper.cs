// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Persian.Plus.PaymentGateway.Core;
using Persian.Plus.PaymentGateway.Core.Gateway;
using Persian.Plus.PaymentGateway.Core.Http;
using Persian.Plus.PaymentGateway.Core.Internal;
using Persian.Plus.PaymentGateway.Core.Options;
using Persian.Plus.PaymentGateway.Core.Storage.Abstractions.Models;
using Persian.Plus.PaymentGateway.Core.Utilities;
using Persian.Plus.PaymentGateway.Gateways.Pasargad.Helper;
using Persian.Plus.PaymentGateway.Gateways.Pasargad.Models;

namespace Persian.Plus.PaymentGateway.Gateways.Pasargad.Soap
{
    internal static class PasargadSoapHelper
    {
        private const string ActionNumber = "1003";
        private const string RefundNumber = "1004";

        public static PaymentRequestResult CreateRequestResult(
            Invoice invoice,
            HttpContext httpContext,
            PasargadSoapGatewayAccount account,
            IPasargadCrypto crypto,
            PasargadSoapGatewayOptions soapGatewayOptions)
        {
            var invoiceDate = GetTimeStamp(DateTime.Now);

            var timeStamp = invoiceDate;

            var dataToSign = string.Format("#{0}#{1}#{2}#{3}#{4}#{5}#{6}#{7}#",
                account.MerchantCode,
                account.TerminalCode,
                invoice.TrackingNumber,
                invoiceDate,
                (long)invoice.Amount,
                invoice.CallbackUrl,
                ActionNumber,
                timeStamp);

            var signedData = crypto.Encrypt(account.PrivateKey, dataToSign);

            var result = PaymentRequestResult.SucceedWithPost(
                account.Name,
                soapGatewayOptions.PaymentPageUrl,
                new Dictionary<string, string>
                {
                    {"merchantCode", account.MerchantCode},
                    {"terminalCode", account.TerminalCode},
                    {"invoiceNumber", invoice.TrackingNumber.ToString()},
                    {"invoiceDate", invoiceDate},
                    {"amount", invoice.Amount.ToLongString()},
                    {"redirectAddress", invoice.CallbackUrl},
                    {"action", ActionNumber},
                    {"timeStamp", timeStamp},
                    {"sign", signedData}
                }, null);

            //result.DatabaseAdditionalData.Add("timeStamp", timeStamp);

            return result;
        }

        public static async Task<PasargadCallbackResult> CreateCallbackResult(
            HttpRequest httpRequest,
            MessagesOptions messagesOptions,
            CancellationToken cancellationToken)
        {
            //  Reference ID
            var invoiceNumber = await httpRequest.TryGetParamAsync("iN", cancellationToken).ConfigureAwaitFalse();

            //  Invoice Date
            var invoiceDate = await httpRequest.TryGetParamAsync("iD", cancellationToken).ConfigureAwaitFalse();

            //  Transaction Code
            var transactionId = await httpRequest.TryGetParamAsync("tref", cancellationToken).ConfigureAwaitFalse();

            var isSucceed = true;
            string message = null;

            if (string.IsNullOrWhiteSpace(invoiceNumber.Value) ||
                string.IsNullOrWhiteSpace(invoiceDate.Value) ||
                string.IsNullOrWhiteSpace(transactionId.Value))
            {
                isSucceed = false;

                message = messagesOptions.InvalidDataReceivedFromGateway;
            }

            var data = new[] { new KeyValuePair<string, string>("invoiceUID", transactionId.Value) };

            return new PasargadCallbackResult
            {
                IsSucceed = isSucceed,
                InvoiceNumber = invoiceNumber.Value,
                InvoiceDate = invoiceDate.Value,
                TransactionId = transactionId.Value,
                CallbackCheckData = data,
                Message = message
            };
        }

        public static PasargadCheckCallbackResult CreateCheckCallbackResult(string webServiceResponse, PasargadSoapGatewayAccount account, PasargadCallbackResult callbackResult, MessagesOptions messagesOptions)
        {
            var compareReferenceId = XmlHelper.GetNodeValueFromXml(webServiceResponse, "invoiceNumber");
            var compareAction = XmlHelper.GetNodeValueFromXml(webServiceResponse, "action");
            var compareMerchantCode = XmlHelper.GetNodeValueFromXml(webServiceResponse, "merchantCode");
            var compareTerminalCode = XmlHelper.GetNodeValueFromXml(webServiceResponse, "terminalCode");

            bool isSucceed;
            PaymentVerifyResult verifyResult = null;

            if (compareReferenceId.IsNullOrWhiteSpace() ||
                compareAction.IsNullOrWhiteSpace() ||
                compareMerchantCode.IsNullOrWhiteSpace() ||
                compareTerminalCode.IsNullOrWhiteSpace())
            {
                isSucceed = false;

                verifyResult = PaymentVerifyResult.Failed(messagesOptions.InvalidDataReceivedFromGateway);
            }
            else
            {
                var responseResult = XmlHelper.GetNodeValueFromXml(webServiceResponse, "result");

                isSucceed = responseResult.Equals("true", StringComparison.OrdinalIgnoreCase) &&
                            compareReferenceId == callbackResult.InvoiceNumber &&
                            compareAction == ActionNumber &&
                            compareMerchantCode == account.MerchantCode &&
                            compareTerminalCode == account.TerminalCode;

                if (!isSucceed)
                {
                    verifyResult = PaymentVerifyResult.Failed("پرداخت موفقيت آميز نبود و يا توسط خريدار کنسل شده است");
                }
            }

            return new PasargadCheckCallbackResult
            {
                IsSucceed = isSucceed,
                Result = verifyResult
            };
        }

        public static IEnumerable<KeyValuePair<string, string>> CreateVerifyData(
            InvoiceContext context,
            PasargadSoapGatewayAccount account,
            IPasargadCrypto crypto,
            PasargadCallbackResult callbackResult)
        {
            var timeStamp = GetTimeStamp(DateTime.Now);

            var dataToSign = string.Format("#{0}#{1}#{2}#{3}#{4}#{5}#",
                account.MerchantCode,
                account.TerminalCode,
                context.Payment.TrackingNumber,
                callbackResult.InvoiceDate,
                (long)context.Payment.Amount,
                timeStamp);

            var signData = crypto.Encrypt(account.PrivateKey, dataToSign);

            return new[]
            {
                new KeyValuePair<string, string>("InvoiceNumber", context.Payment.TrackingNumber.ToString()),
                new KeyValuePair<string, string>("InvoiceDate", callbackResult.InvoiceDate),
                new KeyValuePair<string, string>("MerchantCode", account.MerchantCode),
                new KeyValuePair<string, string>("TerminalCode", account.TerminalCode),
                new KeyValuePair<string, string>("Amount", ((long)context.Payment.Amount).ToString()),
                new KeyValuePair<string, string>("TimeStamp", timeStamp),
                new KeyValuePair<string, string>("Sign", signData)
            };
        }

        public static PaymentVerifyResult CreateVerifyResult(string webServiceResponse, PasargadCallbackResult callbackResult, MessagesOptions messagesOptions)
        {
            var result = XmlHelper.GetNodeValueFromXml(webServiceResponse, "result");

            var isSucceed = result.Equals("true", StringComparison.OrdinalIgnoreCase);

            var message = isSucceed
                ? messagesOptions.PaymentSucceed
                : XmlHelper.GetNodeValueFromXml(webServiceResponse, "resultMessage");

            return new PaymentVerifyResult
            {
                Status = isSucceed ? PaymentVerifyResultStatus.Succeed : PaymentVerifyResultStatus.Failed,
                TransactionCode = callbackResult.TransactionId,
                Message = message
            };
        }

        public static IEnumerable<KeyValuePair<string, string>> CreateRefundData(
            InvoiceContext context,
            Money amount,
            IPasargadCrypto crypto,
            PasargadSoapGatewayAccount account)
        {
            var transactionRecord = context.Transactions.FirstOrDefault(transaction => transaction.Type == TransactionType.Request);

            if (transactionRecord == null)
            {
                throw new Exception($"Cannot find transaction record for Payment-{context.Payment.TrackingNumber}");
            }

            if (!AdditionalDataConverter.ToDictionary(transactionRecord).TryGetValue("invoiceDate", out var invoiceDate))
            {
                throw new Exception("Cannot get the invoiceDate from database.");
            }

            var timeStamp = GetTimeStamp(DateTime.Now);

            var dataToSign = string.Format("#{0}#{1}#{2}#{3}#{4}#{5}#{6}#",
                account.MerchantCode,
                account.TerminalCode,
                context.Payment.TrackingNumber,
                invoiceDate,
                (long)amount,
                RefundNumber,
                timeStamp);

            var signedData = crypto.Encrypt(account.PrivateKey, dataToSign);

            return new[]
            {
                new KeyValuePair<string, string>("InvoiceNumber", context.Payment.TrackingNumber.ToString()),
                new KeyValuePair<string, string>("InvoiceDate", invoiceDate),
                new KeyValuePair<string, string>("MerchantCode", account.MerchantCode),
                new KeyValuePair<string, string>("TerminalCode", account.TerminalCode),
                new KeyValuePair<string, string>("Amount", amount.ToLongString()),
                new KeyValuePair<string, string>("action", RefundNumber),
                new KeyValuePair<string, string>("TimeStamp", timeStamp),
                new KeyValuePair<string, string>("Sign", signedData)
            };
        }

        public static PaymentRefundResult CreateRefundResult(string webServiceResponse, MessagesOptions messagesOptions)
        {
            var result = XmlHelper.GetNodeValueFromXml(webServiceResponse, "result");

            var isSucceed = result.Equals("true", StringComparison.OrdinalIgnoreCase);

            var message = isSucceed
                ? messagesOptions.PaymentSucceed
                : XmlHelper.GetNodeValueFromXml(webServiceResponse, "resultMessage");

            return new PaymentRefundResult
            {
                Status = isSucceed ? PaymentRefundResultStatus.Succeed : PaymentRefundResultStatus.Failed,
                Message = message
            };
        }

        private static string GetTimeStamp(DateTime dateTime)
        {
            return dateTime.ToString("yyyy/MM/dd HH:mm:ss");
        }
    }
}
