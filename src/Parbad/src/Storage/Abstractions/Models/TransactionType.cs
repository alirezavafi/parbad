// Copyright (c) Parbad.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Storage.Abstractions.Models
{
    /// <summary>
    /// Type of transaction.
    /// </summary>
    public enum TransactionType : byte
    {
        Request = 0,
        Callback = 10,
        Verify = 20,
        Canceled = 21,
        Refund = 30
    }
}
