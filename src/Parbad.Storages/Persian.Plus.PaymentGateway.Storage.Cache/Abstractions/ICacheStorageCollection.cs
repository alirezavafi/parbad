﻿// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Persian.Plus.PaymentGateway.Core.Storage.Abstractions.Models;

namespace Persian.Plus.PaymentGateway.Storage.Cache.Abstractions
{
    /// <summary>
    /// A collection for holding the data.
    /// </summary>
    public interface ICacheStorageCollection
    {
        /// <summary>
        /// Gets or sets a list of current payment records.
        /// </summary>
        List<Payment> Payments { get; set; }

        /// <summary>
        /// Gets or sets a list of current transaction records.
        /// </summary>
        List<Transaction> Transactions { get; set; }
    }
}
