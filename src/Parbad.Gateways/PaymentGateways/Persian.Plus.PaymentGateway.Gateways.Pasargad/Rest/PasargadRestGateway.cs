// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Persian.Plus.PaymentGateway.Core;
using Persian.Plus.PaymentGateway.Core.Gateway;
using Persian.Plus.PaymentGateway.Core.Internal;
using Persian.Plus.PaymentGateway.Core.Net;
using Persian.Plus.PaymentGateway.Core.Options;
using Persian.Plus.PaymentGateway.Core.Storage.Abstractions.Models;
using Persian.Plus.PaymentGateway.Gateways.Pasargad.Helper;
using Persian.Plus.PaymentGateway.Gateways.Pasargad.Models;
using Persian.Plus.PaymentGateway.Gateways.Pasargad.Rest.Model;

namespace Persian.Plus.PaymentGateway.Gateways.Pasargad.Rest
{
    [Gateway(Name)]
    public class PasargadRestGateway : GatewayBase<PasargadRestGatewayAccount>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly PasargadRestGatewayOptions _restGatewayOptions;
        private readonly IOptions<MessagesOptions> _messageOptions;
        private readonly IPasargadCrypto _crypto;

        public const string Name = "PasargadRest";

        public PasargadRestGateway(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IGatewayAccountProvider<PasargadRestGatewayAccount> accountProvider,
            IOptions<PasargadRestGatewayOptions> gatewayOptions,
            IOptions<MessagesOptions> messageOptions,
            IPasargadCrypto crypto
        ) : base(accountProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClientFactory.CreateClient(this);
            _httpClient.BaseAddress = new Uri(gatewayOptions.Value.ApiUrl);
            _restGatewayOptions = gatewayOptions.Value;
            _messageOptions = messageOptions;
            _crypto = crypto;
        }

        /// <inheritdoc />
        public override async Task<PaymentRequestResult> RequestAsync(Invoice invoice,
            CancellationToken cancellationToken = default)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice)); 
            var account = await GetAccountAsync(invoice).ConfigureAwaitFalse();

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/Api/v1/Payment/GetToken");
            JsonContent jsonContent;
            if (!string.IsNullOrWhiteSpace(invoice.MobileNumber))
            {
                jsonContent = JsonContent.Create(new
                {
                    InvoiceNumber = invoice.TrackingNumber,
                    InvoiceDate = DateTime.Now.ToString("yyyy/MM/dd"),
                    TerminalCode = account.TerminalCode,
                    MerchantCode = account.MerchantCode,
                    Amount = (long) invoice.Amount,
                    RedirectAddress = invoice.CallbackUrl.Url,
                    Timestamp = ((DateTime)invoice.Properties["CreatedOn"]).ToString("yyyyMMdd HHmmss"),
                    Action = 1003,
                    Mobile = invoice.MobileNumber
                });
            }
            else
            {
                jsonContent = JsonContent.Create(new
                {
                    InvoiceNumber = invoice.TrackingNumber,
                    InvoiceDate = DateTime.Now.ToString("yyyy/MM/dd"),
                    TerminalCode = account.TerminalCode,
                    MerchantCode = account.MerchantCode,
                    Amount = (long) invoice.Amount,
                    RedirectAddress = invoice.CallbackUrl.Url,
                    Timestamp = ((DateTime)invoice.Properties["CreatedOn"]).ToString("yyyyMMdd HHmmss"),
                    Action = 1003,
                });
            }
            requestMessage.Content = jsonContent;
            var dataToSign = await jsonContent.ReadAsStringAsync(cancellationToken);
            var sign = _crypto.Encrypt(account.PrivateKey, dataToSign);
            requestMessage.Headers.Add("Sign", sign);

            var r = await _httpClient.SendAsync(requestMessage, cancellationToken);
            r.EnsureSuccessStatusCode();
            var tokenResult = await r.Content.ReadFromJsonAsync<TokenResultResponse>(cancellationToken: cancellationToken);
            if (tokenResult == null || !tokenResult.IsSuccess)
                throw new InvalidOperationException("cannot get token");
            //JsonConvert.DeserializeObject<JObject>(token);
            return PaymentRequestResult.SucceedWithPost(
                account.Name,
                _restGatewayOptions.PaymentPageUrl,
                new Dictionary<string, string>
                {
                    {"Token", tokenResult.Token},
                },
                tokenResult.Token);
        }

        /// <inheritdoc />
        public override async Task<PaymentFetchResult> FetchAsync(InvoiceContext context,
            CancellationToken cancellationToken = default)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var callbackResult = await GetCallbackResult(context, cancellationToken);
            if (callbackResult.IsSucceed)
            {
                return PaymentFetchResult.ReadyForVerifying(callbackResult);
            }

            return PaymentFetchResult.Failed(callbackResult, callbackResult.Message);
        }

        private async Task<PasargadCallbackResult> GetCallbackResult(InvoiceContext context,
            CancellationToken cancellationToken)
        {
            var callBackTransaction = context.Transactions.SingleOrDefault(x => x.Type == TransactionType.Callback);

            var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();
            PasargadCallbackResult callbackResult;
            if (callBackTransaction == null)
            {
                var invoiceDate = context.Transactions.First().DateTime.ToString("yyyy/MM/dd");
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/Api/v1/Payment/CheckTransactionResult");
                var jsonContent = JsonContent.Create(new
                {
                    InvoiceNumber = context.Payment.TrackingNumber,
                    InvoiceDate = invoiceDate,
                    TerminalCode = account.TerminalCode,
                    MerchantCode = account.MerchantCode,
                });
                requestMessage.Content = jsonContent;
                var dataToSign = await jsonContent.ReadAsStringAsync(cancellationToken);
                var sign = _crypto.Encrypt(account.PrivateKey, dataToSign);
                requestMessage.Headers.Add("Sign", sign);
                var result = await _httpClient.SendAsync(requestMessage, cancellationToken: cancellationToken);

                bool isSucceed = result.IsSuccessStatusCode;
                TransactionResultResponse resultData = null;
                if (isSucceed)
                {
                    resultData =
                        await result.Content.ReadFromJsonAsync<TransactionResultResponse>(
                            cancellationToken: cancellationToken);
                    isSucceed = resultData?.IsSuccess ?? false;
                }

                callbackResult = new PasargadCallbackResult
                {
                    IsSucceed = isSucceed,
                    InvoiceDate = invoiceDate,
                    InvoiceNumber = context.Payment.TrackingNumber.ToString(),
                    TransactionId = resultData?.ReferenceNumber.ToString(),
                    Message = isSucceed ? "موفق" : "ناموفق",
                    CallbackCheckData = new []{new KeyValuePair<string, string>("TransactionReferenceNumber", resultData?.TransactionReferenceId)}
                };
            }
            else
            {
                callbackResult =
                    JsonConvert.DeserializeObject<PasargadCallbackResult>(callBackTransaction.AdditionalData);
            }

            return callbackResult;
        }


        /// <inheritdoc />
        public override async Task<PaymentVerifyResult> VerifyAsync(InvoiceContext context,
            CancellationToken cancellationToken = default)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var callbackResult = await GetCallbackResult(context, cancellationToken);

            if (!callbackResult.IsSucceed)
            {
                return PaymentVerifyResult.Failed(callbackResult.Message);
            }

            var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();
            var verifyHttpRequestMsg = new HttpRequestMessage(HttpMethod.Post, "/Api/v1/Payment/VerifyPayment");
            var invoiceDate = context.Transactions.First().DateTime.ToString("yyyy/MM/dd");
            var jsonContent = JsonContent.Create(new
            {
                InvoiceNumber = context.Payment.TrackingNumber,
                InvoiceDate = invoiceDate,
                TerminalCode = account.TerminalCode,
                MerchantCode = account.MerchantCode,
                Amount = context.Payment.Amount,
                TimeStamp = context.Payment.CreatedOn.ToString("yyyy/MM/dd HH:mm:ss")
            });
            verifyHttpRequestMsg.Content = jsonContent;
            var dataToSign = await jsonContent.ReadAsStringAsync(cancellationToken);
            var sign = _crypto.Encrypt(account.PrivateKey, dataToSign);
            verifyHttpRequestMsg.Headers.Add("Sign", sign);

            var verifyHttpResp = await _httpClient.SendAsync(verifyHttpRequestMsg, cancellationToken);
            if (!verifyHttpResp.IsSuccessStatusCode)
            {
                return new PaymentVerifyResult()
                {
                    IsSucceed = false,
                    Message = "ناموفق",
                };
            }
            var result = await verifyHttpResp.Content.ReadFromJsonAsync<TransactionVerifyResponse>(cancellationToken: cancellationToken);
            if (result == null)
            {
                return new PaymentVerifyResult()
                {
                    IsSucceed = false,
                    Message = "ناموفق",
                };
            }

            return new PaymentVerifyResult()
            {
                IsSucceed = result.IsSuccess,
                Message = result.IsSuccess ? "موفق" : "ناموفق",
                CardNo = result.MaskedCardNumber?.Replace("-", ""),
                Status = result.IsSuccess ? PaymentVerifyResultStatus.Succeed : PaymentVerifyResultStatus.Failed,
                Amount = context.Payment.Amount,
                TransactionCode = result.ShaparakRefNumber,
                TrackingNumber = context.Payment.TrackingNumber,
            };
        }

        /// <inheritdoc />
        public override Task<PaymentRefundResult> RefundAsync(InvoiceContext context, Money amount,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }
}