// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Persian.Plus.PaymentGateway.Core.Storage.Abstractions.Models;
using Persian.Plus.PaymentGateway.Storage.Cache.Abstractions;

namespace Persian.Plus.PaymentGateway.Storage.Cache.Internal
{
    /// <inheritdoc />
    [Serializable]
    public class CacheStorageCollection : ICacheStorageCollection
    {
        /// <summary>
        /// Initializes an instance of <see cref="CacheStorageCollection"/>.
        /// </summary>
        public CacheStorageCollection()
        {
            Payments = new List<Payment>();
            Transactions = new List<Transaction>();
        }

        /// <inheritdoc />
        public List<Payment> Payments { get; set; }

        /// <inheritdoc />
        public List<Transaction> Transactions { get; set; }
    }
}
