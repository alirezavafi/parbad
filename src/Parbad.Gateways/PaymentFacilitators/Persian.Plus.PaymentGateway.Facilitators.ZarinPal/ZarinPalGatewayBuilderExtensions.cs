﻿// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Persian.Plus.PaymentGateway.Core.Gateway;

namespace Persian.Plus.PaymentGateway.Facilitators.ZarinPal
{
    public static class ZarinPalGatewayBuilderExtensions
    {
        /// <summary>
        /// Adds the ZarinPal gateway to Persian.Plus.PaymentGateway.Core services.
        /// </summary>
        /// <param name="builder"></param>
        public static IGatewayConfigurationBuilder<ZarinPalGateway> AddZarinPal(this IGatewayBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder
                .AddGateway<ZarinPalGateway>()
                .WithHttpClient(clientBuilder => { })
                .WithOptions(options => { });
        }

        /// <summary>
        /// Configures the accounts for <see cref="ZarinPalGateway"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureAccounts">Configures the accounts.</param>
        public static IGatewayConfigurationBuilder<ZarinPalGateway> WithAccounts(
            this IGatewayConfigurationBuilder<ZarinPalGateway> builder,
            Action<IGatewayAccountBuilder<ZarinPalGatewayAccount>> configureAccounts)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithAccounts(configureAccounts);
        }

        /// <summary>
        /// Configures the options for ZarinPal Gateway.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions">Configuration</param>
        public static IGatewayConfigurationBuilder<ZarinPalGateway> WithOptions(
            this IGatewayConfigurationBuilder<ZarinPalGateway> builder,
            Action<ZarinPalGatewayOptions> configureOptions)
        {
            builder.Services.Configure(configureOptions);

            return builder;
        }
    }
}
