// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Persian.Plus.PaymentGateway.Core.Gateway;
using Persian.Plus.PaymentGateway.Gateways.Pasargad.Helper;

namespace Persian.Plus.PaymentGateway.Gateways.Pasargad.Rest
{
    public static class PasargadRestGatewayBuilderExtensions
    {
        /// <summary>
        /// Adds Pasargad gateway to Persian.Plus.PaymentGateway.Core services.
        /// </summary>
        /// <param name="builder"></param>
        public static IGatewayConfigurationBuilder<PasargadRestGateway> AddPasargadRest(this IGatewayBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            builder.Services.AddSingleton<IPasargadCrypto, PasargadCrypto>();

            return builder
                .AddGateway<PasargadRestGateway>()
                .WithOptions(options => { })
                .WithHttpClient(clientBuilder => clientBuilder.ConfigureHttpClient(client => { }));
        }

        /// <summary>
        /// Configures the accounts for <see cref="PasargadRestGateway"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureAccounts">Configures the accounts.</param>
        public static IGatewayConfigurationBuilder<PasargadRestGateway> WithAccounts(
            this IGatewayConfigurationBuilder<PasargadRestGateway> builder,
            Action<IGatewayAccountBuilder<PasargadRestGatewayAccount>> configureAccounts)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithAccounts(configureAccounts);
        }

        /// <summary>
        /// Configures the options for Pasargad Gateway.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions">Configuration</param>
        public static IGatewayConfigurationBuilder<PasargadRestGateway> WithOptions(
            this IGatewayConfigurationBuilder<PasargadRestGateway> builder,
            Action<PasargadRestGatewayOptions> configureOptions)
        {
            builder.Services.Configure(configureOptions);

            return builder;
        }
    }
}
