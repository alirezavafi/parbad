using System.Collections.Generic;
using System.Threading.Tasks;
using Persian.Plus.PaymentGateway.Core.Gateway;

namespace Persian.Plus.PaymentGateway.Core.Internal
{
    /// <inheritdoc />
    public class InMemoryGatewayAccountSource<TAccount> : IGatewayAccountSource<TAccount>
        where TAccount : GatewayAccount
    {
        /// <inheritdoc />
        public InMemoryGatewayAccountSource(IEnumerable<TAccount> inMemoryAccounts)
        {
            InMemoryAccounts = inMemoryAccounts;
        }

        /// <summary>
        /// Accounts.
        /// </summary>
        public IEnumerable<TAccount> InMemoryAccounts { get; }

        /// <inheritdoc />
        public Task AddAccountsAsync(IGatewayAccountCollection<TAccount> accounts)
        {
            foreach (var account in InMemoryAccounts)
            {
                accounts.Add(account);
            }

            return Task.CompletedTask;
        }
    }
}
