// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;

namespace Persian.Plus.PaymentGateway.Core.Builder
{
    public static class ParbadBuilderExtensions
    {
        /// <summary>
        /// Adds Persian.Plus.PaymentGateway.Core pre-configured services to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        public static IParbadBuilder AddParbad(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            return ParbadBuilder.CreateDefaultBuilder(services);
        }
    }
}
