// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Persian.Plus.PaymentGateway.Core.Gateway;
using Persian.Plus.PaymentGateway.Core.Net;

namespace Persian.Plus.PaymentGateway.Core.Internal
{
    internal class GatewayConfigurationBuilder<TGateway> : IGatewayConfigurationBuilder<TGateway>
        where TGateway : class, IGateway
    {
        public GatewayConfigurationBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }

        public IGatewayConfigurationBuilder<TGateway> WithAccounts<TAccount>(Action<IGatewayAccountBuilder<TAccount>> configureAccounts)
            where TAccount : GatewayAccount, new()
        {
            if (configureAccounts == null) throw new ArgumentNullException(nameof(configureAccounts));

            configureAccounts(new GatewayAccountBuilder<TAccount>(Services));

            Services
                .TryAddTransient<
                    IGatewayAccountProvider<TAccount>,
                    GatewayAccountProvider<TAccount>>();

            return this;
        }

        public IGatewayConfigurationBuilder<TGateway> WithHttpClient(Action<IHttpClientBuilder> configureHttpClient)
        {
            if (configureHttpClient == null) throw new ArgumentNullException(nameof(configureHttpClient));

            var httpClientBuilder = Services.AddHttpClientForGateway<TGateway>();

            configureHttpClient(httpClientBuilder);

            return this;
        }
    }
}
