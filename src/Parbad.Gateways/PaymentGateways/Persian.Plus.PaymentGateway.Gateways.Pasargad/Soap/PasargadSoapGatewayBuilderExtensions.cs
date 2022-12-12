// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Persian.Plus.PaymentGateway.Core.Gateway;
using Persian.Plus.PaymentGateway.Gateways.Pasargad.Internal;

namespace Persian.Plus.PaymentGateway.Gateways.Pasargad
{
    public static class PasargadSoapGatewayBuilderExtensions
    {
        /// <summary>
        /// Adds Pasargad gateway to Persian.Plus.PaymentGateway.Core services.
        /// </summary>
        /// <param name="builder"></param>
        public static IGatewayConfigurationBuilder<PasargadSoapGateway> AddPasargadSoap(this IGatewayBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.Services.AddSingleton<IPasargadCrypto, PasargadCrypto>();

            return builder
                .AddGateway<PasargadSoapGateway>()
                .WithHttpClient(clientBuilder => { })
                .WithOptions(options => { });
        }

        /// <summary>
        /// Configures the accounts for <see cref="PasargadSoapGateway"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureAccounts">Configures the accounts.</param>
        public static IGatewayConfigurationBuilder<PasargadSoapGateway> WithAccounts(
            this IGatewayConfigurationBuilder<PasargadSoapGateway> builder,
            Action<IGatewayAccountBuilder<PasargadSoapGatewayAccount>> configureAccounts)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithAccounts(configureAccounts);
        }

        /// <summary>
        /// Configures the options for Pasargad Gateway.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions">Configuration</param>
        public static IGatewayConfigurationBuilder<PasargadSoapGateway> WithOptions(
            this IGatewayConfigurationBuilder<PasargadSoapGateway> builder,
            Action<PasargadSoapGatewayOptions> configureOptions)
        {
            builder.Services.Configure(configureOptions);

            return builder;
        }
    }
}
