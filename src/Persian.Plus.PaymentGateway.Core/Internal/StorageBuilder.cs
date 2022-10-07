// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Persian.Plus.PaymentGateway.Core.Storage.Abstractions;

namespace Persian.Plus.PaymentGateway.Core.Internal
{
    internal class StorageBuilder : IStorageBuilder
    {
        public StorageBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }
    }


}
