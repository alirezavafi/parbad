﻿// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Persian.Plus.PaymentGateway.Core.Storage.Abstractions;

namespace Persian.Plus.PaymentGateway.Storage.Cache.MemoryCache
{
    public static class MemoryCacheStorageBuilderExtensions
    {
        /// <summary>
        /// Uses <see cref="IMemoryCache"/> for saving and loading data.
        /// <para>Note: The information inside the memory will be removed
        /// if the website or server goes down for any reasons. Use MemoryCache only for development.</para>
        /// </summary>
        /// <param name="builder"></param>
        public static IStorageBuilder UseMemoryCache(this IStorageBuilder builder)
            => UseMemoryCache(builder, options => { });

        /// <summary>
        /// Uses <see cref="IMemoryCache"/> for saving and loading data.
        /// <para>Note: The information inside the memory will be removed
        /// if the website or server goes down for any reasons. Use MemoryCache only for development.</para>
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions"></param>
        public static IStorageBuilder UseMemoryCache(this IStorageBuilder builder, Action<MemoryCacheStorageOptions> configureOptions)
        {
            builder.Services.AddMemoryCache();

            builder.Services.Configure(configureOptions);

            builder.AddStorage<MemoryCachePaymentStorage>(ServiceLifetime.Transient);

            return builder;
        }
    }
}
