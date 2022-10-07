// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Persian.Plus.PaymentGateway.Core.Builder;
using Persian.Plus.PaymentGateway.Core.Gateway;
using Persian.Plus.PaymentGateway.Core.Internal;
using Persian.Plus.PaymentGateway.Core.Invoice;
using Persian.Plus.PaymentGateway.Core.Options;
using Persian.Plus.PaymentGateway.Core.PaymentTokenProviders;

namespace Persian.Plus.PaymentGateway.Core
{
    /// <inheritdoc />
    public class ParbadBuilder : IParbadBuilder
    {
        /// <summary>
        /// Initializes an instance of <see cref="ParbadBuilder"/> class with the given <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services"></param>
        public ParbadBuilder(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }

        /// <inheritdoc />
        public IServiceCollection Services { get; }

        /// <inheritdoc />
        public IOnlinePaymentAccessor Build()
        {
            var serviceProvider = Services.BuildServiceProvider();

            var onlinePaymentAccessor = serviceProvider.GetRequiredService<IOnlinePaymentAccessor>();

            StaticOnlinePayment.Initialize(onlinePaymentAccessor);

            return onlinePaymentAccessor;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IParbadBuilder"/> class with pre-configured services.
        /// </summary>
        public static IParbadBuilder CreateDefaultBuilder(IServiceCollection services = null)
        {
            services ??= new ServiceCollection();

            var builder = new ParbadBuilder(services);

            builder.Services.AddOptions();

            builder.Services.AddHttpClient();

            builder.Services.TryAddTransient<IOnlinePayment, DefaultOnlinePayment>();
            builder.Services.TryAddSingleton<IOnlinePaymentAccessor, OnlinePaymentAccessor>();

            builder.Services.TryAddTransient<IInvoiceBuilder, DefaultInvoiceBuilder>();

            builder.Services.TryAddTransient<IGatewayProvider, DefaultGatewayProvider>();

            builder.ConfigureOptions(options => { });

            builder.ConfigurePaymentToken(tokenBuilder => tokenBuilder.UseGuidQueryStringPaymentTokenProvider());

            return builder;
        }
    }
}
