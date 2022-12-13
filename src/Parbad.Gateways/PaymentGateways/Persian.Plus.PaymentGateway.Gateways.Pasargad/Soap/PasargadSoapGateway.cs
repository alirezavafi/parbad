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
using Persian.Plus.PaymentGateway.Gateways.Pasargad.Helper;
using Persian.Plus.PaymentGateway.Gateways.Pasargad.Models;

namespace Persian.Plus.PaymentGateway.Gateways.Pasargad.Soap
{
    [Gateway(Name)]
    public class PasargadSoapGateway : GatewayBase<PasargadSoapGatewayAccount>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly IPasargadCrypto _crypto;
        private readonly PasargadSoapGatewayOptions _soapGatewayOptions;
        private readonly IOptions<MessagesOptions> _messageOptions;

        public const string Name = "PasargadSoap";

        public PasargadSoapGateway(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IGatewayAccountProvider<PasargadSoapGatewayAccount> accountProvider,
            IPasargadCrypto crypto,
            IOptions<PasargadSoapGatewayOptions> gatewayOptions,
            IOptions<MessagesOptions> messageOptions) : base(accountProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClientFactory.CreateClient(this);
            _crypto = crypto;
            _soapGatewayOptions = gatewayOptions.Value;
            _messageOptions = messageOptions;
        }

        /// <inheritdoc />
        public override async Task<PaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            var account = await GetAccountAsync(invoice).ConfigureAwaitFalse();

            return PasargadSoapHelper.CreateRequestResult(invoice, _httpContextAccessor.HttpContext, account, _crypto, _soapGatewayOptions);
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

        private async Task<PasargadCallbackResult> GetCallbackResult(InvoiceContext context, CancellationToken cancellationToken)
        {
            var callBackTransaction = context.Transactions.SingleOrDefault(x => x.Type == TransactionType.Callback);

            var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();
            PasargadCallbackResult callbackResult;
            if (callBackTransaction == null)
            {
                callbackResult =  await PasargadSoapHelper.CreateCallbackResult(
                        _httpContextAccessor.HttpContext.Request,
                        _messageOptions.Value,
                        cancellationToken)
                    .ConfigureAwaitFalse();
            }
            else
            {
                callbackResult =
                    JsonConvert.DeserializeObject<PasargadCallbackResult>(callBackTransaction.AdditionalData);
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

            var responseMessage = await _httpClient.PostFormAsync(
                    _soapGatewayOptions.ApiCheckPaymentUrl,
                    callbackResult.CallbackCheckData,
                    cancellationToken)
                .ConfigureAwaitFalse();

            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();

            var checkCallbackResult = PasargadSoapHelper.CreateCheckCallbackResult(
                response,
                account,
                callbackResult,
                _messageOptions.Value);

            if (!checkCallbackResult.IsSucceed)
            {
                return checkCallbackResult.Result;
            }

            var data = PasargadSoapHelper.CreateVerifyData(context, account, _crypto, callbackResult);

            responseMessage = await _httpClient.PostFormAsync(
                    _soapGatewayOptions.ApiVerificationUrl,
                    data,
                    cancellationToken)
                .ConfigureAwaitFalse();

            response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            return PasargadSoapHelper.CreateVerifyResult(response, callbackResult, _messageOptions.Value);
        }

        /// <inheritdoc />
        public override async Task<PaymentRefundResult> RefundAsync(InvoiceContext context, Money amount, CancellationToken cancellationToken = default)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();

            var data = PasargadSoapHelper.CreateRefundData(context, amount, _crypto, account);

            var responseMessage = await _httpClient.PostFormAsync(
                    _soapGatewayOptions.ApiRefundUrl,
                    data,
                    cancellationToken)
                .ConfigureAwaitFalse();

            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            return PasargadSoapHelper.CreateRefundResult(response, _messageOptions.Value);
        }
    }
}
