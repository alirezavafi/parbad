// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;
using Persian.Plus.PaymentGateway.Core.Builder;
using Persian.Plus.PaymentGateway.Core.Internal;

namespace Persian.Plus.PaymentGateway.Core.Http
{
    public static class HttpContextBuilderExtensions
    {
        /// <summary>
        /// Configures the <see cref="HttpContext"/> required by Persian.Plus.PaymentGateway.Core.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="httpContextBuilder">A builder for configuring the HttpContext.</param>
        public static IParbadBuilder ConfigureHttpContext(this IParbadBuilder builder, Action<IHttpContextBuilder> httpContextBuilder)
        {
            httpContextBuilder(new HttpContextBuilder(builder.Services));

            return builder;
        }
    }
}
