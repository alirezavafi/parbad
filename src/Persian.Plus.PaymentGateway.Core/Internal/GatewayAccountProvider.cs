﻿// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Persian.Plus.PaymentGateway.Core.Gateway;

namespace Persian.Plus.PaymentGateway.Core.Internal
{
    public class GatewayAccountProvider<TAccount> : IGatewayAccountProvider<TAccount> where TAccount : GatewayAccount
    {
        private readonly IEnumerable<IGatewayAccountSource<TAccount>> _sources;

        public GatewayAccountProvider(IEnumerable<IGatewayAccountSource<TAccount>> sources)
        {
            _sources = sources;
        }

        public virtual async Task<IGatewayAccountCollection<TAccount>> LoadAccountsAsync()
        {
            var accounts = new GatewayAccountCollection<TAccount>();

            foreach (var source in _sources)
            {
                await source.AddAccountsAsync(accounts);
            }

            return accounts;
        }
    }
}
