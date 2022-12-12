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
using Newtonsoft.Json.Linq;
using Persian.Plus.PaymentGateway.Core;
using Persian.Plus.PaymentGateway.Core.Gateway;
using Persian.Plus.PaymentGateway.Core.Internal;
using Persian.Plus.PaymentGateway.Core.Net;
using Persian.Plus.PaymentGateway.Core.Options;
using Persian.Plus.PaymentGateway.Core.Storage.Abstractions.Models;
using Persian.Plus.PaymentGateway.Gateways.AsanPardakht.Rest.Model;
using Persian.Plus.PaymentGateway.Gateways.Pasargad;
using Persian.Plus.PaymentGateway.Gateways.Pasargad.Internal.Models;

namespace Persian.Plus.PaymentGateway.Gateways.AsanPardakht.Rest
{
    [Gateway(Name)]
    public class PasardgadRestGateway : GatewayBase<PasargadRestGatewayAccount>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly PasargadRestGatewayOptions _restGatewayOptions;
        private readonly IOptions<MessagesOptions> _messageOptions;
        private readonly IPasargadCrypto _crypto;

        public const string Name = "PasargadRest";

        public PasardgadRestGateway(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IGatewayAccountProvider<PasargadRestGatewayAccount> accountProvider,
            IOptions<PasargadRestGatewayOptions> gatewayOptions,
            IOptions<MessagesOptions> messageOptions,
            IPasargadCrypto crypto,
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

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/v1/Token");
            var jsonContent = JsonContent.Create(new
            {
                InvoiceNumber = invoice.TrackingNumber,
                InvoiceDate = DateTime.Now.ToString("yyyyMMdd HHmmss"),
                TerminalCode = account.TerminalCode,
                MerchantCode = account.MerchantCode,
                Amount = (long) invoice.Amount,
                RedirectAddress = invoice.CallbackUrl.Url,
                Timestamp = DateTime.Now.ToString("yyyyMMdd HHmmss"),
                Action = 1003,
                Mobile = invoice.MobileNumber
            });
            requestMessage.Content = jsonContent;
            var dataToSign = await jsonContent.ReadAsStringAsync();
            var sign = _crypto.Encrypt(account.PrivateKey, dataToSign);
            requestMessage.Headers.Add("Sign", sign);

            var r = await _httpClient.SendAsync(requestMessage, cancellationToken);
            r.EnsureSuccessStatusCode();
            var token = await r.Content.ReadAsStringAsync(cancellationToken);
            //JsonConvert.DeserializeObject<JObject>(token);
            return PaymentRequestResult.SucceedWithPost(
                account.Name,
                _restGatewayOptions.PaymentPageUrl,
                new Dictionary<string, string>
                {
                    {"Token", token},
                },
                token);
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
                var requestMessage = new HttpRequestMessage(HttpMethod.Get,
                    $"/v1/TranResult?LocalInvoiceId={context.Payment.TrackingNumber}&MerchantConfigurationId={account.MerchantConfigurationId}");
                requestMessage.Headers.Add("usr", account.UserName);
                requestMessage.Headers.Add("pwd", account.Password);
                var result = await _httpClient.SendAsync(requestMessage, cancellationToken: cancellationToken);

                bool isSucceed = result.IsSuccessStatusCode;
                TransactionResultResponse resultData = null;
                if (isSucceed)
                    resultData =
                        await result.Content.ReadFromJsonAsync<TransactionResultResponse>(
                            cancellationToken: cancellationToken);

                callbackResult = new PasargadCallbackResult
                {
                    IsSucceed = isSucceed,
                    PayGateTranId = resultData?.PayGateTranId,
                    Rrn = resultData?.Rrn,
                    CardNumber = resultData?.CardNumber,
                    Message = isSucceed ? "موفق" : "ناموفق",
                    Status = resultData?.ServiceStatusCode
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
            var verifyHttpRequestMsg = new HttpRequestMessage(HttpMethod.Post, "/v1/Verify");
            verifyHttpRequestMsg.Headers.Add("usr", account.UserName);
            verifyHttpRequestMsg.Headers.Add("pwd", account.Password);
            verifyHttpRequestMsg.Content = JsonContent.Create(new
            {
                merchantConfigurationId = account.MerchantConfigurationId,
                payGateTranId = callbackResult.PayGateTranId
            });

            var verifyHttpResp = await _httpClient.SendAsync(verifyHttpRequestMsg, cancellationToken);
            if (!verifyHttpResp.IsSuccessStatusCode)
            {
                return new PaymentVerifyResult()
                {
                    IsSucceed = false,
                    Message = "ناموفق",
                };
            }

            var settleHttpRequestMsg = new HttpRequestMessage(HttpMethod.Post, "/v1/Settlement");
            settleHttpRequestMsg.Headers.Add("usr", account.UserName);
            settleHttpRequestMsg.Headers.Add("pwd", account.Password);
            settleHttpRequestMsg.Content = JsonContent.Create(new
            {
                merchantConfigurationId = account.MerchantConfigurationId,
                payGateTranId = callbackResult.PayGateTranId
            });

            if (!verifyHttpResp.IsSuccessStatusCode)
            {
                return new PaymentVerifyResult()
                {
                    IsSucceed = false,
                    Message = "ناموفق",
                };
            }

            var settleHttpResp = await _httpClient.SendAsync(settleHttpRequestMsg, cancellationToken);

            var verifyResult = new PaymentVerifyResult
            {
                Status = settleHttpResp.IsSuccessStatusCode ? PaymentVerifyResultStatus.Succeed : PaymentVerifyResultStatus.Failed,
                TransactionCode = callbackResult.Rrn,
                Message = settleHttpResp.IsSuccessStatusCode ? "موفق" : "ناموفق"
            };

            //verifyResult.DatabaseAdditionalData.Add("PayGateTranId", callbackResult.PayGateTranId);
            //verifyResult.DatabaseAdditionalData.Add("LastFourDigitOfPAN", callbackResult.CardNumber);

            return verifyResult;
        }

        /// <inheritdoc />
        public override Task<PaymentRefundResult> RefundAsync(InvoiceContext context, Money amount,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }
}