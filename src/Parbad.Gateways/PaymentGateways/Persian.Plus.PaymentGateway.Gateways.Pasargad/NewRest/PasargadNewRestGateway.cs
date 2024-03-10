// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
using Persian.Plus.PaymentGateway.Gateways.Pasargad.Models;
using Persian.Plus.PaymentGateway.Gateways.Pasargad.NewRest.Model;
using Persian.Plus.PaymentGateway.Gateways.Pasargad.Rest.Model;

namespace Persian.Plus.PaymentGateway.Gateways.Pasargad.NewRest
{
    [Gateway(Name)]
    public class PasargadNewRestGateway : GatewayBase<PasargadNewRestGatewayAccount>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly PasargadNewRestGatewayOptions _restGatewayOptions;
        private readonly IOptions<MessagesOptions> _messageOptions;

        public const string Name = "PasargadNewRest";

        public PasargadNewRestGateway(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IGatewayAccountProvider<PasargadNewRestGatewayAccount> accountProvider,
            IOptions<PasargadNewRestGatewayOptions> gatewayOptions,
            IOptions<MessagesOptions> messageOptions
        ) : base(accountProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClientFactory.CreateClient(this);
            _httpClient.BaseAddress = new Uri(gatewayOptions.Value.ApiUrl);
            _restGatewayOptions = gatewayOptions.Value;
            _messageOptions = messageOptions;
        }

        private static Dictionary<string, string> TokensCache = new();
        private static object fake = new object();
        private async Task<string> GetToken(PasargadNewRestGatewayAccount account)
        {
            if (TokensCache.TryGetValue(account.TerminalNumber, out var token))
            {
                return token;
            }

            var token2 = await GetNewToken(account);
            TokensCache.TryAdd(account.TerminalNumber, token2);
            return TokensCache[account.TerminalNumber];
        }

        public void RemoveTokenFromCache(PasargadNewRestGatewayAccount account)
        {
            if (TokensCache.ContainsKey(account.TerminalNumber))
            {
                TokensCache.Remove(account.TerminalNumber);
            }
        }
        
        
        private async Task<string> GetNewToken(PasargadNewRestGatewayAccount account)
        {
            var resp = await _httpClient.PostJsonAsync("token/getToken", new
            {
                username = account.Username,
                password = account.Password
            });
            resp.EnsureSuccessStatusCode();
            var res = await resp.Content.ReadFromJsonAsync<TokenResponse>();
            return res.Token;
        }

        /// <inheritdoc />
        public override async Task<PaymentRequestResult> RequestAsync(Invoice invoice,
            CancellationToken cancellationToken = default)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice)); 
            var account = await GetAccountAsync(invoice).ConfigureAwaitFalse();

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "api/payment/purchase");
            var invoiceCreationDate = ((DateTime)invoice.Properties["CreatedOn"]);
            var jsonContent = JsonContent.Create(new
            {
                invoice = invoice.TrackingNumber,
                invoiceDate = invoiceCreationDate.ToString("yyyy/MM/dd HH:mm:ss"),
                serviceCode = 8,
                serviceType = "PURCHASE",
                terminalNumber = account.TerminalNumber,
                amount = (long) invoice.Amount,
                callbackApi = invoice.CallbackUrl.Url,
                mobileNumber = invoice.MobileNumber,
                pans = invoice.CardNumber
            });
            requestMessage.Content = jsonContent;
            var token = await GetToken(account);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var r = await _httpClient.SendAsync(requestMessage, cancellationToken);
            if (r.StatusCode == HttpStatusCode.Unauthorized || r.StatusCode == HttpStatusCode.Forbidden)
            {
                RemoveTokenFromCache(account);
                var token2 = await GetToken(account);
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token2);
                r = await _httpClient.SendAsync(requestMessage, cancellationToken);
            }
            r.EnsureSuccessStatusCode();
            var resp = await r.Content.ReadFromJsonAsync<PepPurchaseResponse>(cancellationToken: cancellationToken);
            if (resp == null || resp.ResultCode != 0)
                throw new InvalidOperationException("cannot get token");
            //JsonConvert.DeserializeObject<JObject>(token);
            return PaymentRequestResult.SucceedWithRedirect(
                account.Name,
                resp.Data.Url,
                new Dictionary<string, string>
                {
                    { "Url", resp.Data.Url },
                    { "UrlId", resp.Data.UrlId },
                });
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

        private async Task<PasargadNewRestCallbackResult> GetCallbackResult(InvoiceContext context,
            CancellationToken cancellationToken)
        {
            var callBackTransaction = context.Transactions.SingleOrDefault(x => x.Type == TransactionType.Callback);

            var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();
            PasargadNewRestCallbackResult callbackResult;
            if (callBackTransaction == null)
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, "api/payment/payment-inquiry");
                var jsonContent = JsonContent.Create(new
                {
                    invoiceId = context.Payment.TrackingNumber,
                });
                requestMessage.Content = jsonContent;
                var token = await GetToken(account);
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var result = await _httpClient.SendAsync(requestMessage, cancellationToken: cancellationToken);
                if (result.StatusCode == HttpStatusCode.Unauthorized || result.StatusCode == HttpStatusCode.Forbidden)
                {
                    RemoveTokenFromCache(account);
                    var token2 = await GetToken(account);
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token2);
                    result = await _httpClient.SendAsync(requestMessage, cancellationToken);
                }

                bool isSucceed = result.IsSuccessStatusCode;
                PepInquiryResponse resultData = null;
                if (isSucceed)
                {
                    resultData =
                        await result.Content.ReadFromJsonAsync<PepInquiryResponse>(
                            cancellationToken: cancellationToken);
                    isSucceed = resultData?.Data?.Status == 0;
                }

                callbackResult = new PasargadNewRestCallbackResult
                {
                    IsSucceed = isSucceed,
                    InvoiceNumber = context.Payment.TrackingNumber.ToString(),
                    TransactionId = resultData?.Data?.TransactionId,
                    Message = isSucceed ? "موفق" : "ناموفق",
                    CallbackInquiryResult = resultData?.Data
                };
            }
            else
            {
                callbackResult =
                    JsonConvert.DeserializeObject<PasargadNewRestCallbackResult>(callBackTransaction.AdditionalData);
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
            var tr = context.Transactions.OrderByDescending(x => x.Id).First(x => x.Type == TransactionType.Request);
            var requestData = JsonConvert.DeserializeObject<PaymentRequestResult>(tr.AdditionalData);
            var verifyHttpRequestMsg = new HttpRequestMessage(HttpMethod.Post, "api/payment/verify-payment");
            var jsonContent = JsonContent.Create(new
            {
                invoice = context.Payment.TrackingNumber,
                urlId = ((JToken)requestData.GatewayResult)["UrlId"]?.ToString()
            });
            verifyHttpRequestMsg.Content = jsonContent;
            var token = await GetToken(account);
            verifyHttpRequestMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var verifyHttpResp = await _httpClient.SendAsync(verifyHttpRequestMsg, cancellationToken);
            if (verifyHttpResp.StatusCode == HttpStatusCode.Unauthorized || verifyHttpResp.StatusCode == HttpStatusCode.Forbidden)
            {
                RemoveTokenFromCache(account);
                var token2 = await GetToken(account);
                verifyHttpRequestMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token2);
                verifyHttpResp = await _httpClient.SendAsync(verifyHttpRequestMsg, cancellationToken);
            }
            if (!verifyHttpResp.IsSuccessStatusCode)
            {
                return new PaymentVerifyResult()
                {
                    IsSucceed = false,
                    Message = "ناموفق",
                };
            }
            var result = await verifyHttpResp.Content.ReadFromJsonAsync<PepVerifyResponse>(cancellationToken: cancellationToken);
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
                IsSucceed = result.ResultCode == 0,
                Message = result.ResultCode == 0 ? "موفق" : "ناموفق",
                CardNo = result.Data?.MaskedCardNumber?.Replace("-", ""),
                Status = result.ResultCode == 0 ? PaymentVerifyResultStatus.Succeed : PaymentVerifyResultStatus.Failed,
                Amount = context.Payment.Amount,
                TransactionCode = result.Data?.ReferenceNumber,
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