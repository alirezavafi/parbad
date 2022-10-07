// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Persian.Plus.PaymentGateway.Core.Storage.Abstractions;

namespace Persian.Plus.PaymentGateway.Storage.Cache.DistributedCache
{
    public static class DistributedCacheStorageBuilderExtensions
    {
        /// <summary>
        /// Uses <see cref="IDistributedCache"/> for saving and loading data.
        /// </summary>
        /// <param name="builder"></param>
        public static IStorageBuilder UseDistributedCache(this IStorageBuilder builder)
            => UseDistributedCache(builder, options => { });

        /// <summary>
        /// Uses <see cref="IDistributedCache"/> for saving and loading data.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions"></param>
        public static IStorageBuilder UseDistributedCache(this IStorageBuilder builder, Action<DistributedCacheStorageOptions> configureOptions)
        {
            builder.Services.Configure(configureOptions);
            builder.AddStorage<DistributedCachePaymentStorage>(ServiceLifetime.Transient);

            return builder;
        }
    }
}
