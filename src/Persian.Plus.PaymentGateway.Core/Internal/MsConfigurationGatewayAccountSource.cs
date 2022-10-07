using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Persian.Plus.PaymentGateway.Core.Gateway;

namespace Persian.Plus.PaymentGateway.Core.Internal
{
    public class MsConfigurationGatewayAccountSource<TAccount> : IGatewayAccountSource<TAccount>
        where TAccount : GatewayAccount, new()
    {
        public MsConfigurationGatewayAccountSource(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public Task AddAccountsAsync(IGatewayAccountCollection<TAccount> accounts)
        {
            var newAccount = new TAccount();

            Configuration.Bind(newAccount);

            accounts.Add(newAccount);

            return Task.CompletedTask;
        }
    }
}
