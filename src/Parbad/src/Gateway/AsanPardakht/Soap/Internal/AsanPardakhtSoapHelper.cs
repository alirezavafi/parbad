// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;
using Parbad.Abstraction;
using Parbad.Gateway.AsanPardakht.Internal.Models;
using Parbad.Internal;
using Parbad.Options;
using Parbad.Utilities;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Parbad.Gateway.AsanPardakht.Internal
{
    internal static class AsanPardakhtSoapHelper
    {
        //public const string PaymentPageUrl = "https://asan.shaparak.ir/";
        //public const string BaseServiceUrl = "https://ipgsoap.asanpardakht.ir/paygate/merchantservices.asmx";

        public static string CreateRequestData(Invoice invoice, AsanPardakhtSoapGatewayAccount account, IAsanPardakhtSoapCrypto soapCrypto)
        {
            var requestToEncrypt = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}",
                1,
                account.UserName,
                account.Password,
                invoice.TrackingNumber,
                invoice.Amount.ToLongString(),
                DateTime.Now.ToString("yyyyMMdd HHmmss"),
                "",
                invoice.CallbackUrl,
                "0"
            );

            var encryptedRequest = soapCrypto.Encrypt(requestToEncrypt, account.Key, account.IV);

            return
                "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tem=\"http://tempuri.org/\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<tem:RequestOperation>" +
                $"<tem:merchantConfigurationID>{account.MerchantConfigurationId}</tem:merchantConfigurationID>" +
                "<!--Optional:-->" +
                $"<tem:encryptedRequest>{XmlHelper.EncodeXmlValue(encryptedRequest)}</tem:encryptedRequest>" +
                "</tem:RequestOperation>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }

        public static PaymentRequestResult CreateRequestResult(
            string response,
            AsanPardakhtSoapGatewayAccount account,
            HttpContext httpContext,
            AsanPardakhtSoapGatewayOptions soapGatewayOptions,
            MessagesOptions messagesOptions)
        {
            var result = XmlHelper.GetNodeValueFromXml(response, "RequestOperationResult", "http://tempuri.org/");

            var splitedResult = result.Split(',');

            var isSucceed = splitedResult.Length == 2 && splitedResult[0] == "0";

            if (!isSucceed)
            {
                var message = AsanPardakhtResultTranslator.TranslateRequest(splitedResult[0], messagesOptions);

                return PaymentRequestResult.Failed(message, account.Name);
            }

            return PaymentRequestResult.SucceedWithPost(
                account.Name,
                httpContext,
                soapGatewayOptions.PaymentPageUrl,
                new Dictionary<string, string>
                {
                    {"RefId", splitedResult[1]}
                }, result);
        }

        public static AsanPardakhtCallbackResult CreateCallbackResult(
            InvoiceContext context,
            AsanPardakhtSoapGatewayAccount account,
            HttpRequest httpRequest,
            IAsanPardakhtSoapCrypto soapCrypto,
            MessagesOptions messagesOptions)
        {
            httpRequest.Form.TryGetValue("ReturningParams", out var returningParams);

            var isSucceed = false;
            string message = null;
            string payGateTranId = null;
            string rrn = null;
            string lastFourDigitOfPAN = null;

            if (returningParams.IsNullOrEmpty())
            {
                isSucceed = false;

                message = messagesOptions.InvalidDataReceivedFromGateway;
            }
            else
            {
                var decryptedResult = soapCrypto.Decrypt(returningParams, account.Key, account.IV);

                var splitedResult = decryptedResult.Split(',');

                var amount = splitedResult[0];
                var preInvoiceID = splitedResult[1];
                var token = splitedResult[2];
                var resCode = splitedResult[3];
                var messageText = splitedResult[4];
                payGateTranId = splitedResult[5];
                rrn = splitedResult[6];
                lastFourDigitOfPAN = splitedResult[7];

                isSucceed = resCode == "0" || resCode == "00";

                if (!isSucceed)
                {
                    message = messageText.IsNullOrEmpty()
                        ? AsanPardakhtResultTranslator.TranslateRequest(resCode, messagesOptions)
                        : messageText;
                }
                else
                {
                    if (long.TryParse(amount, out var longAmount))
                    {
                        if (longAmount != (long)context.Payment.Amount)
                        {
                            isSucceed = false;
                            message = "مبلغ پرداخت شده با مبلغ درخواست شده مطابقت ندارد.";
                        }
                    }
                    else
                    {
                        isSucceed = false;
                        message = "مبلغ پرداخت شده نامشخص است.";
                    }
                }
            }

            return new AsanPardakhtCallbackResult
            {
                IsSucceed = isSucceed,
                PayGateTranId = payGateTranId,
                Rrn = rrn,
                CardNumber = lastFourDigitOfPAN,
                Message = message
            };
        }

        public static string CreateVerifyData(
            AsanPardakhtCallbackResult callbackResult,
            AsanPardakhtSoapGatewayAccount account,
            IAsanPardakhtSoapCrypto soapCrypto)
        {
            var requestToEncrypt = account.UserName + "," + account.Password;
            var encryptedRequest = soapCrypto.Encrypt(requestToEncrypt, account.Key, account.IV);

            return
                "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tem=\"http://tempuri.org/\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<tem:RequestVerification>" +
                $"<tem:merchantConfigurationID>{account.MerchantConfigurationId}</tem:merchantConfigurationID>" +
                "<!--Optional:-->" +
                $"<tem:encryptedCredentials>{XmlHelper.EncodeXmlValue(encryptedRequest)}</tem:encryptedCredentials>" +
                $"<tem:payGateTranID>{callbackResult.PayGateTranId}</tem:payGateTranID>" +
                "</tem:RequestVerification>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }

        public static AsanPardakhtVerifyResult CheckVerifyResult(
            string response,
            AsanPardakhtCallbackResult callbackResult,
            MessagesOptions messagesOptions)
        {
            if (!callbackResult.IsSucceed)
            {
                return new AsanPardakhtVerifyResult
                {
                    IsSucceed = false,
                    Result = PaymentVerifyResult.Failed(callbackResult.Message)
                };
            }

            var result = XmlHelper.GetNodeValueFromXml(response, "RequestVerificationResult", "http://tempuri.org/");

            var isSucceed = result == "500";

            PaymentVerifyResult verifyResult = null;

            if (!isSucceed)
            {
                var message = AsanPardakhtResultTranslator.TranslateVerification(result, messagesOptions);

                verifyResult = PaymentVerifyResult.Failed(message);
                verifyResult.Message = message;
            }

            return new AsanPardakhtVerifyResult
            {
                IsSucceed = isSucceed,
                Result = verifyResult
            };
        }

        public static string CreateSettleData(
            AsanPardakhtCallbackResult callbackResult,
            AsanPardakhtSoapGatewayAccount account,
            IAsanPardakhtSoapCrypto soapCrypto)
        {
            var requestToEncrypt = $"{account.UserName},{account.Password}";
            var encryptedRequest = soapCrypto.Encrypt(requestToEncrypt, account.Key, account.IV);

            return
                "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tem=\"http://tempuri.org/\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<tem:RequestReconciliation>" +
                $"<tem:merchantConfigurationID>{account.MerchantConfigurationId}</tem:merchantConfigurationID>" +
                "<!--Optional:-->" +
                $"<tem:encryptedCredentials>{XmlHelper.EncodeXmlValue(encryptedRequest)}</tem:encryptedCredentials>" +
                $"<tem:payGateTranID>{callbackResult.PayGateTranId}</tem:payGateTranID>" +
                "</tem:RequestReconciliation>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }

        public static PaymentVerifyResult CreateSettleResult(
            string response,
            AsanPardakhtCallbackResult callbackResult,
            MessagesOptions messagesOptions)
        {
            var result = XmlHelper.GetNodeValueFromXml(response, "RequestReconciliationResult", "http://tempuri.org/");

            var isSucceed = result == "600";
            string message;

            if (isSucceed)
            {
                message = messagesOptions.PaymentSucceed;
            }
            else
            {
                message = AsanPardakhtResultTranslator.TranslateReconcilation(result, messagesOptions) ??
                          messagesOptions.PaymentFailed;
            }

            var verifyResult = new PaymentVerifyResult
            {
                Status = isSucceed ? PaymentVerifyResultStatus.Succeed : PaymentVerifyResultStatus.Failed,
                TransactionCode = callbackResult.Rrn,
                Message = message
            };

            verifyResult.DatabaseAdditionalData.Add("PayGateTranId", callbackResult.PayGateTranId);
            verifyResult.DatabaseAdditionalData.Add("LastFourDigitOfPAN", callbackResult.CardNumber);

            return verifyResult;
        }
    }
}
