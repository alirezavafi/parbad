// Copyright (c) Parbad.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Parbad.Gateway.AsanPardakht.Internal;
using Parbad.GatewayBuilders;

namespace Parbad.Gateway.AsanPardakht
{
    public static class AsanPardakhtSoapGatewayBuilderExtensions
    {
        /// <summary>
        /// Adds AsanPardakht gateway to Parbad.Core services.
        /// </summary>
        /// <param name="builder"></param>
        public static IGatewayConfigurationBuilder<AsanPardakhtSoapGateway> AddAsanPardakhtSoap(this IGatewayBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.Services.AddSingleton<IAsanPardakhtSoapCrypto, AsanPardakhtSoapCrypto>();

            return builder
                .AddGateway<AsanPardakhtSoapGateway>()
                .WithOptions(options => { })
                .WithHttpClient(clientBuilder => clientBuilder.ConfigureHttpClient(client => { }));
        }

        /// <summary>
        /// Configures the accounts for <see cref="AsanPardakhtSoapGateway"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureAccounts">Configures the accounts.</param>
        public static IGatewayConfigurationBuilder<AsanPardakhtSoapGateway> WithAccounts(
            this IGatewayConfigurationBuilder<AsanPardakhtSoapGateway> builder,
            Action<IGatewayAccountBuilder<AsanPardakhtSoapGatewayAccount>> configureAccounts)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithAccounts(configureAccounts);
        }

        /// <summary>
        /// Configures the options for AsanPardakht Gateway.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions">Configuration</param>
        public static IGatewayConfigurationBuilder<AsanPardakhtSoapGateway> WithOptions(
            this IGatewayConfigurationBuilder<AsanPardakhtSoapGateway> builder,
            Action<AsanPardakhtSoapGatewayOptions> configureOptions)
        {
            builder.Services.Configure(configureOptions);

            return builder;
        }
    }
}
