﻿// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Persian.Plus.PaymentGateway.Core.Exceptions;

namespace Persian.Plus.PaymentGateway.Core.Gateway
{
    /// <summary>
    /// Describes a gateway account.
    /// </summary>
    public abstract class GatewayAccount
    {
        public static readonly string DefaultName = "Default";

        /// <summary>
        /// Gets or sets the name of this account. The default value is "Default".
        /// <para>Note: Make sure that accounts have different names. Otherwise a <see cref="DuplicateAccountException"/> will throw.</para>
        /// </summary>
        public string Name { get; set; } = DefaultName;
    }
}
