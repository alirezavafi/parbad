// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Persian.Plus.Extensions;
using Persian.Plus.PaymentGateway.Core;
using Persian.Plus.PaymentGateway.Core.Gateway;
using Persian.Plus.PaymentGateway.Core.Http;
using Persian.Plus.PaymentGateway.Core.Internal;
using Persian.Plus.PaymentGateway.Core.Options;
using Persian.Plus.PaymentGateway.Core.Utilities;
using Persian.Plus.PaymentGateway.Gateways.Mellat.Internal.Models;

namespace Persian.Plus.PaymentGateway.Gateways.Mellat.Internal
{
    internal static class MellatHelper
    {
        private const string OkResult = "0";
        private const string DuplicateOrderNumberResult = "41";
        private const string AlreadyVerifiedResult = "43";
        private const string SettleSuccess = "45";

        internal const string CumulativeAccountsKey = "MellatCumulativeAccounts";

        public static string CreateRequestData(Invoice invoice, MellatGatewayAccount account)
        {
            if (!invoice.Properties.ContainsKey(CumulativeAccountsKey))
            {
                return CreateSimpleRequestData(invoice, account);
            }

            return CreateCumulativeRequestData(invoice, account);
        }

        public static PaymentRequestResult CreateRequestResult(
            string webServiceResponse,
            Invoice invoice,
            HttpContext httpContext,
            MellatGatewayOptions gatewayOptions,
            MessagesOptions messagesOptions,
            GatewayAccount account)
        {
            var result = XmlHelper.GetNodeValueFromXml(webServiceResponse, "return");

            var arrayResult = result.Split(',');

            var resCode = arrayResult[0];
            var refId = arrayResult.Length > 1 ? arrayResult[1] : string.Empty;

            var isSucceed = resCode == OkResult;

            if (!isSucceed)
            {
                var message = resCode == DuplicateOrderNumberResult
                    ? messagesOptions.DuplicateTrackingNumber
                    : MellatGatewayResultTranslator.Translate(resCode, messagesOptions);

                return PaymentRequestResult.Failed(message, account.Name);
            }

            var form = new Dictionary<string, string>
            {
                {"RefId", refId}
            };

            var mobileNumber = invoice.CoercedMobileNumber();
            if (!string.IsNullOrEmpty(mobileNumber))
                form.Add("MobileNo", mobileNumber);
            if (invoice.Properties.ContainsKey("EncryptedPan"))
            {
                form.Add("HiddenMode", "0");
                form.Add("EncPan", invoice.Properties["EncryptedPan"].ToString());
            }

            return PaymentRequestResult.SucceedWithPost(
                account.Name,
                gatewayOptions.PaymentPageUrl,
                form,
                webServiceResponse);
        }

        public static async Task<MellatCallbackResult> CreateCallbackResultAsync(
            HttpRequest httpRequest,
            MessagesOptions messagesOptions,
            CancellationToken cancellationToken)
        {
            var resCode = await httpRequest.TryGetParamAsync("ResCode", cancellationToken).ConfigureAwaitFalse();

            if (!resCode.Exists || resCode.Value.IsNullOrEmpty())
            {
                return new MellatCallbackResult
                {
                    IsSucceed = false,
                    Message = messagesOptions.InvalidDataReceivedFromGateway
                };
            }

            //  Reference ID
            var refId = await httpRequest.TryGetParamAsync("RefId", cancellationToken).ConfigureAwaitFalse();

            //  Transaction Code
            var saleReferenceId = await httpRequest.TryGetParamAsync("SaleReferenceId", cancellationToken).ConfigureAwaitFalse();
            
            var cardNo = await httpRequest.TryGetParamAsync("CardHolderPAN", cancellationToken).ConfigureAwaitFalse();

            var isSucceed = resCode.Value == OkResult;

            string message = null;

            if (!isSucceed)
            {
                message = MellatGatewayResultTranslator.Translate(resCode.Value, messagesOptions);
            }

            return new MellatCallbackResult
            {
                IsSucceed = isSucceed,
                RefId = refId.Value,
                SaleReferenceId = saleReferenceId.Value,
                Message = message,
                CardNo = cardNo.Value,
            };
        }

        public static string CreateVerifyData(InvoiceContext context, MellatGatewayAccount account, MellatCallbackResult callbackResult)
        {
            return
                "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:int=\"http://interfaces.core.sw.bps.com/\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<int:bpVerifyRequest>" +
                $"<terminalId>{account.TerminalId}</terminalId>" +
                "<!--Optional:-->" +
                $"<userName>{account.UserName}</userName>" +
                "<!--Optional:-->" +
                $"<userPassword>{account.UserPassword}</userPassword>" +
                $"<orderId>{context.Payment.TrackingNumber}</orderId>" +
                $"<saleOrderId>{context.Payment.TrackingNumber}</saleOrderId>" +
                $"<saleReferenceId>{callbackResult.SaleReferenceId}</saleReferenceId>" +
                "</int:bpVerifyRequest>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }

        public static MellatVerifyResult CheckVerifyResult(string webServiceResponse, MellatCallbackResult callbackResult, MessagesOptions messagesOptions)
        {
            var serviceResult = XmlHelper.GetNodeValueFromXml(webServiceResponse, "return");

            var isSucceed = serviceResult == OkResult;

            PaymentVerifyResult verifyResult = null;

            if (!isSucceed)
            {
                var message = MellatGatewayResultTranslator.Translate(serviceResult, messagesOptions);

                verifyResult = PaymentVerifyResult.Failed(message);
            }

            return new MellatVerifyResult
            {
                IsSucceed = isSucceed,
                Result = verifyResult
            };
        }

        public static string CreateSettleData(InvoiceContext context, MellatCallbackResult callbackResult, MellatGatewayAccount account)
        {
            return
                "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:int=\"http://interfaces.core.sw.bps.com/\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<int:bpSettleRequest>" +
                $"<terminalId>{account.TerminalId}</terminalId>" +
                "<!--Optional:-->" +
                $"<userName>{account.UserName}</userName>" +
                "<!--Optional:-->" +
                $"<userPassword>{account.UserPassword}</userPassword>" +
                $"<orderId>{context.Payment.TrackingNumber}</orderId>" +
                $"<saleOrderId>{context.Payment.TrackingNumber}</saleOrderId>" +
                $"<saleReferenceId>{callbackResult.SaleReferenceId}</saleReferenceId>" +
                "</int:bpSettleRequest>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }

        public static PaymentVerifyResult CreateSettleResult(string webServiceResponse, MellatCallbackResult callbackResult, MessagesOptions messagesOptions)
        {
            var result = XmlHelper.GetNodeValueFromXml(webServiceResponse, "return");

            var isSuccess = result == OkResult || result == SettleSuccess;

            var message = isSuccess
                ? messagesOptions.PaymentSucceed
                : MellatGatewayResultTranslator.Translate(result, messagesOptions);

            return new PaymentVerifyResult
            {
                Status = isSuccess ? PaymentVerifyResultStatus.Succeed : PaymentVerifyResultStatus.Failed,
                TransactionCode = callbackResult.SaleReferenceId,
                CardNo = callbackResult.CardNo,
                Message = message,
            };
        }

        public static string CreateRefundData(InvoiceContext context, MellatGatewayAccount account)
        {
            return
                "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:int=\"http://interfaces.core.sw.bps.com/\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<int:bpReversalRequest>" +
                $"<terminalId>{account.TerminalId}</terminalId>" +
                "<!--Optional:-->" +
                $"<userName>{account.UserName}</userName>" +
                "<!--Optional:-->" +
                $"<userPassword>{account.UserPassword}</userPassword>" +
                $"<orderId>{context.Payment.TrackingNumber}</orderId>" +
                $"<saleOrderId>{context.Payment.TrackingNumber}</saleOrderId>" +
                $"<saleReferenceId>{context.Payment.TransactionCode}</saleReferenceId>" +
                "</int:bpReversalRequest>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }

        public static PaymentRefundResult CreateRefundResult(string webServiceResponse, MessagesOptions messagesOptions)
        {
            var result = XmlHelper.GetNodeValueFromXml(webServiceResponse, "return");

            var isSuccess = result == OkResult;

            var message = MellatGatewayResultTranslator.Translate(result, messagesOptions);

            return new PaymentRefundResult
            {
                Status = isSuccess ? PaymentRefundResultStatus.Succeed : PaymentRefundResultStatus.Failed,
                Message = message
            };
        }

        private static string CreateSimpleRequestData(Invoice invoice, MellatGatewayAccount account)
        {
            var mobileNumberSoapPart = String.Empty;
            var mobileNumber = invoice.CoercedMobileNumber();
            if (!string.IsNullOrWhiteSpace(mobileNumber))
                mobileNumberSoapPart = $"<mobileNo>{mobileNumber}</mobileNo>";
            var panSoapPart = String.Empty;
            if (invoice.Properties.ContainsKey("EncryptedPan"))
            {
                var encryptedPan = invoice.Properties["EncryptedPan"].ToString();
                panSoapPart = $"<encPan>{encryptedPan}</encPan>";
            }

            return
                "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:int=\"http://interfaces.core.sw.bps.com/\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<int:bpPayRequest>" +
                $"<terminalId>{account.TerminalId}</terminalId>" +
                "<!--Optional:-->" +
                $"<userName>{account.UserName}</userName>" +
                "<!--Optional:-->" +
                $"<userPassword>{account.UserPassword}</userPassword>" +
                $"<orderId>{invoice.TrackingNumber}</orderId>" +
                $"<amount>{(long)invoice.Amount}</amount>" +
                "<!--Optional:-->" +
                $"<localDate>{System.DateTime.Now:yyyyMMdd}</localDate>" +
                "<!--Optional:-->" +
                $"<localTime>{System.DateTime.Now:HHmmss}</localTime>" +
                "<!--Optional:-->" +
                "<additionalData></additionalData>" +
                "<!--Optional:-->" +
                $"<callBackUrl>{XmlHelper.EncodeXmlValue(invoice.CallbackUrl)}</callBackUrl>" +
                "<payerId>0</payerId>" + mobileNumberSoapPart + panSoapPart +
                "</int:bpPayRequest>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }
        
        private static string CoercedMobileNumber(this Invoice i)
        {
            return i.MobileNumber?.CoerceIranianMobileNumber()[1..]?.Insert(0, "98");
        }

        private static string CreateCumulativeRequestData(Invoice invoice, MellatGatewayAccount account)
        {
            var cumulativeAccounts = (List<MellatCumulativeDynamicAccount>)invoice.Properties[CumulativeAccountsKey];

            if (cumulativeAccounts.Count > 10)
            {
                throw new Exception("Cannot use more than 10 accounts for each Cumulative payment request.");
            }

            //var totalAmount = cumulativeAccounts.Sum(cumulativeAccount => cumulativeAccount.Amount);

            //if (totalAmount != invoice.Amount)
            //{
            //    throw new Exception("The total amount of Mellat Cumulative accounts is not equals to the amount of the invoice." +
            //                        $"Invoice amount: {invoice.Amount}." +
            //                        $"Accounts total amount: {totalAmount}");
            //}

            var additionalData = cumulativeAccounts.Aggregate("", (current, cumulativeAccount) => current + $"{cumulativeAccount};");

            return
                "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:int=\"http://interfaces.core.sw.bps.com/\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<int:bpCumulativeDynamicPayRequest>" +
                $"<terminalId>{account.TerminalId}</terminalId>" +
                "<!--Optional:-->" +
                $"<userName>{account.UserName}</userName>" +
                "<!--Optional:-->" +
                $"<userPassword>{account.UserPassword}</userPassword>" +
                $"<orderId>{invoice.TrackingNumber}</orderId>" +
                $"<amount>{(long)invoice.Amount}</amount>" +
                "<!--Optional:-->" +
                $"<localDate>{System.DateTime.Now:yyyyMMdd}</localDate>" +
                "<!--Optional:-->" +
                $"<localTime>{System.DateTime.Now:HHmmss}</localTime>" +
                "<!--Optional:-->" +
                $"<additionalData>{additionalData}</additionalData>" +
                "<!--Optional:-->" +
                $"<callBackUrl>{XmlHelper.EncodeXmlValue(invoice.CallbackUrl)}</callBackUrl>" +
                "</int:bpCumulativeDynamicPayRequest>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }
    }
}
