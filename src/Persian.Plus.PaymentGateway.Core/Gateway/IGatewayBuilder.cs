// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace Persian.Plus.PaymentGateway.Core.Gateway
{
    /// <summary>
    /// A builder for building a gateway.
    /// </summary>
    public interface IGatewayBuilder
    {
        /// <summary>
        /// Specifies the contract for a collection of service descriptors.
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// Adds the specified gateway to Persian.Plus.PaymentGateway.Core services.
        /// </summary>
        /// <typeparam name="TGateway">Type of gateway.</typeparam>
        /// <param name="serviceLifetime">Lifetime of <typeparamref name="TGateway"/>.</param>
        IGatewayConfigurationBuilder<TGateway> AddGateway<TGateway>(
            ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
            where TGateway : class, IGateway;
    }
}
