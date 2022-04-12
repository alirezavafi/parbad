﻿// Copyright (c) Parbad.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Parbad.Abstraction;
using Parbad.Gateway.Sepehr.Internal;
using Parbad.GatewayBuilders;
using Parbad.Internal;
using Parbad.Net;
using Parbad.Options;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Parbad.Storage.Abstractions.Models;

namespace Parbad.Gateway.Sepehr
{
    /// <summary>
    /// Sepehr Gateway.
    /// </summary>
    [Gateway(Name)]
    public class SepehrGateway : GatewayBase<SepehrGatewayAccount>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly SepehrGatewayOptions _gatewayOptions;
        private readonly ParbadOptions _options;

        public const string Name = "Sepehr";

        private static JsonSerializerSettings DefaultSerializerSettings => new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        /// <summary>
        /// Initializes an instance of <see cref="SepehrGateway"/>.
        /// </summary>
        public SepehrGateway(
            IGatewayAccountProvider<SepehrGatewayAccount> accountProvider,
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IOptions<SepehrGatewayOptions> gatewayOptions,
            IOptions<ParbadOptions> options) : base(accountProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClientFactory.CreateClient(this);
            _gatewayOptions = gatewayOptions.Value;
            _options = options.Value;
        }

        /// <inheritdoc />
        public override async Task<PaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            var account = await GetAccountAsync(invoice).ConfigureAwaitFalse();

            var data = SepehrHelper.CreateRequestData(invoice, account);

            var responseMessage = await _httpClient
                .PostJsonAsync(_gatewayOptions.ApiTokenUrl, data, DefaultSerializerSettings, cancellationToken)
                .ConfigureAwaitFalse();

            return await SepehrHelper.CreateRequestResult(responseMessage, _httpContextAccessor.HttpContext, account, _gatewayOptions, _options.Messages);
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

            return PaymentFetchResult.Failed(callbackResult, callbackResult.Message);
        }

        private async Task<CallbackResultModel> GetCallbackResult(InvoiceContext context, CancellationToken cancellationToken)
        {
            var callBackTransaction = context.Transactions.SingleOrDefault(x => x.Type == TransactionType.Callback);

            var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();
            CallbackResultModel callbackResult;
            if (callBackTransaction == null)
            {
                callbackResult = await SepehrHelper.CreateCallbackResultAsync(
                        context,
                        _httpContextAccessor.HttpContext.Request,
                        account,
                        _options.Messages,
                        cancellationToken)
                    .ConfigureAwaitFalse();
            }
            else
            {
                callbackResult =
                    JsonConvert.DeserializeObject<CallbackResultModel>(callBackTransaction.AdditionalData);
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
            var data = SepehrHelper.CreateVerifyData(callbackResult, account);

            var responseMessage = await _httpClient
                .PostJsonAsync(_gatewayOptions.ApiAdviceUrl, data, DefaultSerializerSettings, cancellationToken)
                .ConfigureAwaitFalse();

            return await SepehrHelper.CreateVerifyResult(context, responseMessage, callbackResult, _options.Messages);
        }

        /// <inheritdoc />
        public override async Task<PaymentRefundResult> RefundAsync(InvoiceContext context, Money amount, CancellationToken cancellationToken = default)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();

            var data = SepehrHelper.CreateRefundData(context, account);

            var responseMessage = await _httpClient
                .PostJsonAsync(_gatewayOptions.ApiRollbackUrl, data, DefaultSerializerSettings, cancellationToken)
                .ConfigureAwaitFalse();

            return await SepehrHelper.CreateRefundResult(context, responseMessage, _options.Messages);
        }
    }
}
