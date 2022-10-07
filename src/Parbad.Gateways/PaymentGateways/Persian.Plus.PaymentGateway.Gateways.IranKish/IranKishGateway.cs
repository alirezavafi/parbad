// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Net.Http;
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
using Persian.Plus.PaymentGateway.Gateways.IranKish.Internal;

namespace Persian.Plus.PaymentGateway.Gateways.IranKish
{
    [Gateway(Name)]
    public class IranKishGateway : GatewayBase<IranKishGatewayAccount>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly IranKishGatewayOptions _gatewayOptions;
        private readonly IOptions<MessagesOptions> _messageOptions;

        public const string Name = "IranKish";

        public IranKishGateway(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IGatewayAccountProvider<IranKishGatewayAccount> accountProvider,
            IOptions<IranKishGatewayOptions> gatewayOptions,
            IOptions<MessagesOptions> messageOptions) : base(accountProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClientFactory.CreateClient(this);
            _gatewayOptions = gatewayOptions.Value;
            _messageOptions = messageOptions;
        }

        /// <inheritdoc />
        public override async Task<PaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            var account = await GetAccountAsync(invoice).ConfigureAwaitFalse();

            var data = IranKishHelper.CreateRequestData(invoice, account);

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add(IranKishHelper.HttpRequestHeader.Key, IranKishHelper.HttpRequestHeader.Value);

            var responseMessage = await _httpClient
                .PostXmlAsync(_gatewayOptions.ApiTokenUrl, data, cancellationToken)
                .ConfigureAwaitFalse();

            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            return IranKishHelper.CreateRequestResult(response, account, _gatewayOptions, _httpContextAccessor.HttpContext, _messageOptions.Value);
        }

        /// <inheritdoc />
        public override async Task<PaymentFetchResult> FetchAsync(InvoiceContext context, CancellationToken cancellationToken = default)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var callbackResult = await GetCallbackResult(context, cancellationToken);

            if (callbackResult.IsSucceed)
            {
                return PaymentFetchResult.ReadyForVerifying(callbackResult);
            }

            return PaymentFetchResult.Failed(callbackResult ,callbackResult.Message);
        }

        private async Task<IranKishCallbackResult> GetCallbackResult(InvoiceContext context, CancellationToken cancellationToken)
        {
            var callBackTransaction = context.Transactions.SingleOrDefault(x => x.Type == TransactionType.Callback);

            var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();
            IranKishCallbackResult callbackResult;
            if (callBackTransaction == null)
            {
                callbackResult = await IranKishHelper.CreateCallbackResultAsync(
                    context,
                    account,
                    _httpContextAccessor.HttpContext.Request,
                    _messageOptions.Value,
                    cancellationToken);
            }
            else
            {
                callbackResult =
                    JsonConvert.DeserializeObject<IranKishCallbackResult>(callBackTransaction.AdditionalData);
            }

            return callbackResult;
        }


        /// <inheritdoc />
        public override async Task<PaymentVerifyResult> VerifyAsync(InvoiceContext context, CancellationToken cancellationToken = default)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            
            var callbackResult = await GetCallbackResult(context, cancellationToken);

            if (!callbackResult.IsSucceed)
            {
                return PaymentVerifyResult.Failed(callbackResult.Message);
            }

            var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();
            var data = IranKishHelper.CreateVerifyData(callbackResult, account);

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add(IranKishHelper.HttpVerifyHeader.Key, IranKishHelper.HttpVerifyHeader.Value);

            var responseMessage = await _httpClient
                .PostXmlAsync(_gatewayOptions.ApiVerificationUrl, data, cancellationToken)
                .ConfigureAwaitFalse();

            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            return IranKishHelper.CreateVerifyResult(response, context, callbackResult, _messageOptions.Value);
        }

        /// <inheritdoc />
        public override Task<PaymentRefundResult> RefundAsync(InvoiceContext context, Money amount, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }
}
