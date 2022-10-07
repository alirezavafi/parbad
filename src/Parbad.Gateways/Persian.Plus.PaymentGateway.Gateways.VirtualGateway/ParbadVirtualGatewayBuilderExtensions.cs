// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Persian.Plus.PaymentGateway.Core.Gateway;
using Persian.Plus.PaymentGateway.Gateways.VirtualGateway.MiddlewareInvoker;

namespace Persian.Plus.PaymentGateway.Gateways.VirtualGateway
{
    public static class ParbadVirtualGatewayBuilderExtensions
    {
        /// <summary>
        /// Adds the Persian.Plus.PaymentGateway.Core Virtual Gateway to Persian.Plus.PaymentGateway.Core services.
        /// </summary>
        /// <param name="builder"></param>
        public static IGatewayConfigurationBuilder<ParbadVirtualGateway> AddParbadVirtual(this IGatewayBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.Services.TryAddTransient<IParbadVirtualGatewayMiddlewareInvoker, ParbadVirtualGatewayMiddlewareInvoker>();

            return builder
                .AddGateway<ParbadVirtualGateway>()
                .WithAccounts(accounts => accounts.AddInMemory(account => { }));
        }

        /// <summary>
        /// Configures the accounts for <see cref="ParbadVirtualGateway"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureAccounts">Configures the accounts.</param>
        public static IGatewayConfigurationBuilder<ParbadVirtualGateway> WithAccounts(
            this IGatewayConfigurationBuilder<ParbadVirtualGateway> builder,
            Action<IGatewayAccountBuilder<ParbadVirtualGatewayAccount>> configureAccounts)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithAccounts(configureAccounts);
        }

        /// <summary>
        /// Configures Persian.Plus.PaymentGateway.Core Virtual gateway options.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions"></param>
        public static IGatewayConfigurationBuilder<ParbadVirtualGateway> WithOptions(
            this IGatewayConfigurationBuilder<ParbadVirtualGateway> builder,
            Action<ParbadVirtualGatewayOptions> configureOptions)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.Services.Configure(configureOptions);

            return builder;
        }
    }
}
