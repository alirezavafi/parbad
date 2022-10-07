// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Persian.Plus.PaymentGateway.Core.PaymentTokenProviders;

namespace Persian.Plus.PaymentGateway.Core.Internal
{
    public class GuidQueryStringPaymentTokenProvider : QueryStringTokenProvider
    {
        public GuidQueryStringPaymentTokenProvider(
            IHttpContextAccessor httpContextAccessor,
            IOptions<QueryStringPaymentTokenOptions> options)
        : base(httpContextAccessor, options)
        {
        }

        protected override Task<string> GenerateTokenAsync(Gateway.Invoice invoice, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Guid.NewGuid().ToString("N"));
        }
    }
}
