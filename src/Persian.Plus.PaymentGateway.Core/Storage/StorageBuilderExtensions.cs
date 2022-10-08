// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Persian.Plus.PaymentGateway.Core.Builder;
using Persian.Plus.PaymentGateway.Core.Internal;
using Persian.Plus.PaymentGateway.Core.Storage.Abstractions;

namespace Persian.Plus.PaymentGateway.Core.Storage
{
    public static class StorageBuilderExtensions
    {
        /// <summary>
        /// Configures the storage which required by Persian.Plus.PaymentGateway.Core for saving and loading data.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureStorage"></param>
        public static IPaymentGatewayBuilder ConfigureStorage(this IPaymentGatewayBuilder builder, Action<IStorageBuilder> configureStorage)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (configureStorage == null) throw new ArgumentNullException(nameof(configureStorage));

            var storageBuilder = new StorageBuilder(builder.Services);
            configureStorage(storageBuilder);

            return builder;
        }
    }
}
