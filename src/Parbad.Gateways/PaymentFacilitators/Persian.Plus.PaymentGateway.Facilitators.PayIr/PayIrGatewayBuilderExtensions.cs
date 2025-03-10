﻿// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Persian.Plus.PaymentGateway.Core.Gateway;

namespace Persian.Plus.PaymentGateway.Facilitators.PayIr
{
    public static class PayIrGatewayBuilderExtensions
    {
        /// <summary>
        /// Adds the Pay.ir gateway to Persian.Plus.PaymentGateway.Core services.
        /// </summary>
        /// <param name="builder"></param>
        public static IGatewayConfigurationBuilder<PayIrGateway> AddPayIr(this IGatewayBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder
                .AddGateway<PayIrGateway>()
                .WithHttpClient(clientBuilder => { })
                .WithOptions(options => { });
        }

        /// <summary>
        /// Configures the accounts for Pay.ir.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureAccounts">Configures the accounts.</param>
        public static IGatewayConfigurationBuilder<PayIrGateway> WithAccounts(
            this IGatewayConfigurationBuilder<PayIrGateway> builder,
            Action<IGatewayAccountBuilder<PayIrGatewayAccount>> configureAccounts)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithAccounts(configureAccounts);
        }
        /// <summary>
        /// Configures the options for PayIr Gateway.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions">Configuration</param>
        public static IGatewayConfigurationBuilder<PayIrGateway> WithOptions(
            this IGatewayConfigurationBuilder<PayIrGateway> builder,
            Action<PayIrGatewayOptions> configureOptions)
        {
            builder.Services.Configure(configureOptions);

            return builder;
        }

    }
}
