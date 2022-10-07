// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Persian.Plus.PaymentGateway.Storage.Cache.Abstractions;
using Persian.Plus.PaymentGateway.Storage.Cache.Internal;

namespace Persian.Plus.PaymentGateway.Storage.Cache.DistributedCache
{
    /// <summary>
    /// Distributed cache implementation of Persian.Plus.PaymentGateway.Core storage.
    /// </summary>
    public class DistributedCachePaymentStorage : CachePaymentStorage
    {
        private readonly IDistributedCache _distributedCache;
        private readonly DistributedCacheStorageOptions _options;

        /// <summary>
        /// Initializes an instance of <see cref="DistributedCachePaymentStorage"/>.
        /// </summary>
        /// <param name="distributedCache"></param>
        /// <param name="options"></param>
        public DistributedCachePaymentStorage(IDistributedCache distributedCache, IOptions<DistributedCacheStorageOptions> options)
        {
            _distributedCache = distributedCache;
            _options = options.Value;
            Collection = BuildCollection();
        }

        /// <inheritdoc />
        protected override ICacheStorageCollection Collection { get; }

        /// <inheritdoc />
        protected override Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var data = ObjectSerializer.SerializeObject(Collection);

            return _distributedCache.SetAsync(_options.CacheKey, data, _options.CacheEntryOptions, cancellationToken);
        }

        private ICacheStorageCollection BuildCollection()
        {
            var buffer = _distributedCache.Get(_options.CacheKey);

            return buffer == null
                ? new CacheStorageCollection()
                : ObjectSerializer.DeserializeObject<ICacheStorageCollection>(buffer);
        }
    }
}
