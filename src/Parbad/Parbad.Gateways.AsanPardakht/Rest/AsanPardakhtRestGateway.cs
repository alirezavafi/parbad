// Copyright (c) Parbad.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Parbad.Abstraction;
using Parbad.Gateway.AsanPardakht.Internal;
using Parbad.Gateway.AsanPardakht.Internal.Models;
using Parbad.Gateway.AsanPardakht.Model;
using Parbad.GatewayBuilders;
using Parbad.Internal;
using Parbad.Net;
using Parbad.Options;
using Parbad.Properties;
using Parbad.Storage.Abstractions.Models;

namespace Parbad.Gateway.AsanPardakht
{
    [Gateway(Name)]
    public class AsanPardakhtRestGateway : GatewayBase<AsanPardakhtRestGatewayAccount>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly AsanPardakhtRestGatewayOptions _restGatewayOptions;
        private readonly IOptions<MessagesOptions> _messageOptions;

        public const string Name = "AsanPardakhtRest";

        public AsanPardakhtRestGateway(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IGatewayAccountProvider<AsanPardakhtRestGatewayAccount> accountProvider,
            IOptions<AsanPardakhtRestGatewayOptions> gatewayOptions,
            IOptions<MessagesOptions> messageOptions
        ) : base(accountProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClientFactory.CreateClient(this);
            _httpClient.BaseAddress = new Uri(gatewayOptions.Value.ApiUrl);
            _restGatewayOptions = gatewayOptions.Value;
            _messageOptions = messageOptions;
        }

        /// <inheritdoc />
        public override async Task<IPaymentRequestResult> RequestAsync(Invoice invoice,
            CancellationToken cancellationToken = default)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            var account = await GetAccountAsync(invoice).ConfigureAwaitFalse();

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/v1/Token");
            requestMessage.Headers.Add("usr", account.UserName);
            requestMessage.Headers.Add("pwd", account.Password);
            requestMessage.Content = JsonContent.Create(new
            {
                merchantConfigurationId = account.MerchantConfigurationId,
                serviceTypeId = 1,
                localInvoiceId = invoice.TrackingNumber,
                amountInRials = (long) invoice.Amount,
                localDate = DateTime.Now.ToString("yyyyMMdd HHmmss"),
                callbackURL = invoice.CallbackUrl.Url
            });

            var r = await _httpClient.SendAsync(requestMessage, cancellationToken);
            r.EnsureSuccessStatusCode();
            var token = await r.Content.ReadAsStringAsync(cancellationToken);

            return PaymentRequestResult.SucceedWithPost(
                account.Name,
                _httpContextAccessor.HttpContext,
                _restGatewayOptions.PaymentPageUrl,
                new Dictionary<string, string>
                {
                    {"RefId", token},
                    {"mobileap", invoice.MobileNumber}
                },
                token);
        }

        /// <inheritdoc />
        public override async Task<IPaymentFetchResult> FetchAsync(InvoiceContext context,
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

        private async Task<AsanPardakhtCallbackResult> GetCallbackResult(InvoiceContext context,
            CancellationToken cancellationToken)
        {
            var callBackTransaction = context.Transactions.SingleOrDefault(x => x.Type == TransactionType.Callback);

            var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();
            AsanPardakhtCallbackResult callbackResult;
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

                callbackResult = new AsanPardakhtCallbackResult
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
                    JsonConvert.DeserializeObject<AsanPardakhtCallbackResult>(callBackTransaction.AdditionalData);
            }

            return callbackResult;
        }


        /// <inheritdoc />
        public override async Task<IPaymentVerifyResult> VerifyAsync(InvoiceContext context,
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

            verifyResult.DatabaseAdditionalData.Add("PayGateTranId", callbackResult.PayGateTranId);
            verifyResult.DatabaseAdditionalData.Add("LastFourDigitOfPAN", callbackResult.CardNumber);

            return verifyResult;
        }

        /// <inheritdoc />
        public override Task<IPaymentRefundResult> RefundAsync(InvoiceContext context, Money amount,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }
}