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
using Persian.Plus.PaymentGateway.Gateways.AsanPardakht.Models;
using Persian.Plus.PaymentGateway.Gateways.AsanPardakht.Soap.Internal;

namespace Persian.Plus.PaymentGateway.Gateways.AsanPardakht.Soap
{
    [Gateway(Name)]
    public class AsanPardakhtSoapGateway : GatewayBase<AsanPardakhtSoapGatewayAccount>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly IAsanPardakhtSoapCrypto _soapCrypto;
        private readonly AsanPardakhtSoapGatewayOptions _soapGatewayOptions;
        private readonly IOptions<MessagesOptions> _messageOptions;

        public const string Name = "AsanPardakhtSoap";

        public AsanPardakhtSoapGateway(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IGatewayAccountProvider<AsanPardakhtSoapGatewayAccount> accountProvider,
            IAsanPardakhtSoapCrypto soapCrypto,
            IOptions<AsanPardakhtSoapGatewayOptions> gatewayOptions,
            IOptions<MessagesOptions> messageOptions
            ) : base(accountProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClientFactory.CreateClient(this);
            _soapCrypto = soapCrypto;
            _soapGatewayOptions = gatewayOptions.Value;
            _messageOptions = messageOptions;
        }

        /// <inheritdoc />
        public override async Task<PaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            var account = await GetAccountAsync(invoice).ConfigureAwaitFalse();

            var data = AsanPardakhtSoapHelper.CreateRequestData(invoice, account, _soapCrypto);

            var responseMessage = await _httpClient
                .PostXmlAsync(_soapGatewayOptions.ApiUrl, data, cancellationToken)
                .ConfigureAwaitFalse();

            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            return AsanPardakhtSoapHelper.CreateRequestResult(
                response,
                account,
                _httpContextAccessor.HttpContext,
                _soapGatewayOptions,
                _messageOptions.Value);
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
  
        private async Task<AsanPardakhtCallbackResult> GetCallbackResult(InvoiceContext context, CancellationToken cancellationToken)
        {
            var callBackTransaction = context.Transactions.SingleOrDefault(x => x.Type == TransactionType.Callback);

            var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();
            AsanPardakhtCallbackResult callbackResult;
            if (callBackTransaction == null)
            {
                callbackResult = AsanPardakhtSoapHelper.CreateCallbackResult(
                    context,
                    account,
                    _httpContextAccessor.HttpContext.Request,
                    _soapCrypto,
                    _messageOptions.Value);
            }
            else
            {
                callbackResult =
                    JsonConvert.DeserializeObject<AsanPardakhtCallbackResult>(callBackTransaction.AdditionalData);
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
            var data = AsanPardakhtSoapHelper.CreateVerifyData(callbackResult, account, _soapCrypto);

            var responseMessage = await _httpClient
                .PostXmlAsync(_soapGatewayOptions.ApiUrl, data, cancellationToken)
                .ConfigureAwaitFalse();

            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            var verifyResult = AsanPardakhtSoapHelper.CheckVerifyResult(response, callbackResult, _messageOptions.Value);

            if (!verifyResult.IsSucceed)
            {
                return verifyResult.Result;
            }

            data = AsanPardakhtSoapHelper.CreateSettleData(callbackResult, account, _soapCrypto);

            responseMessage = await _httpClient
                .PostXmlAsync(_soapGatewayOptions.ApiUrl, data, cancellationToken)
                .ConfigureAwaitFalse();

            response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            return AsanPardakhtSoapHelper.CreateSettleResult(response, callbackResult, _messageOptions.Value);
        }

        /// <inheritdoc />
        public override Task<PaymentRefundResult> RefundAsync(InvoiceContext context, Money amount, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }
}
