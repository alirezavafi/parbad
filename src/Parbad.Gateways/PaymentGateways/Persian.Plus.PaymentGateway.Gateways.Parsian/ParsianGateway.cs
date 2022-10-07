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
using Persian.Plus.PaymentGateway.Gateways.Parsian.Internal;
using Persian.Plus.PaymentGateway.Gateways.Parsian.Internal.Models;

namespace Persian.Plus.PaymentGateway.Gateways.Parsian
{
    [Gateway(Name)]
    public class ParsianGateway : GatewayBase<ParsianGatewayAccount>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly ParsianGatewayOptions _gatewayOptions;
        private readonly MessagesOptions _messageOptions;

        public const string Name = "Parsian";

        public ParsianGateway(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IGatewayAccountProvider<ParsianGatewayAccount> accountProvider,
            IOptions<ParsianGatewayOptions> gatewayOptions,
            IOptions<MessagesOptions> messageOptions) : base(accountProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClientFactory.CreateClient(this);
            _gatewayOptions = gatewayOptions.Value;
            _messageOptions = messageOptions.Value;
        }

        /// <inheritdoc />
        public override async Task<PaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            var account = await GetAccountAsync(invoice).ConfigureAwaitFalse();

            var data = ParsianHelper.CreateRequestData(account, invoice);

            var responseMessage = await _httpClient
                .PostXmlAsync(_gatewayOptions.ApiRequestUrl, data, cancellationToken)
                .ConfigureAwaitFalse();

            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            return ParsianHelper.CreateRequestResult(response, _httpContextAccessor.HttpContext, account, _gatewayOptions, _messageOptions);
        }

        /// <inheritdoc />
        public override async Task<PaymentFetchResult> FetchAsync(InvoiceContext context, CancellationToken cancellationToken = default)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var callbackResult = await GetCallbackResult(context, cancellationToken);
            PaymentFetchResult result;

            if (callbackResult.IsSucceed)
            {
                return PaymentFetchResult.ReadyForVerifying(callbackResult);
            }

            return PaymentFetchResult.Failed(callbackResult, callbackResult.Message);
        }

        private async Task<ParsianCallbackResult> GetCallbackResult(InvoiceContext context, CancellationToken cancellationToken)
        {
            var callBackTransaction = context.Transactions.SingleOrDefault(x => x.Type == TransactionType.Callback);

            ParsianCallbackResult callbackResult;
            if (callBackTransaction == null)
            {
                callbackResult = ParsianHelper.CreateCallbackResult(_httpContextAccessor.HttpContext.Request, context, _messageOptions);
            }
            else
            {
                callbackResult =
                    JsonConvert.DeserializeObject<ParsianCallbackResult>(callBackTransaction.AdditionalData);
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

            var data = ParsianHelper.CreateVerifyData(account, callbackResult);

            var responseMessage = await _httpClient
                .PostXmlAsync(_gatewayOptions.ApiVerificationUrl, data, cancellationToken)
                .ConfigureAwaitFalse();

            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            return ParsianHelper.CreateVerifyResult(response, _messageOptions);
        }

        /// <inheritdoc />
        public override async Task<PaymentRefundResult> RefundAsync(InvoiceContext context, Money amount, CancellationToken cancellationToken = default)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();

            var data = ParsianHelper.CreateRefundData(account, context, amount);

            var responseMessage = await _httpClient
                .PostXmlAsync(_gatewayOptions.ApiRefundUrl, data, cancellationToken)
                .ConfigureAwaitFalse();

            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            return ParsianHelper.CreateRefundResult(response, _messageOptions);
        }
    }
}
