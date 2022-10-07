using System;
using Persian.Plus.PaymentGateway.Core.Gateway;

namespace Persian.Plus.PaymentGateway.Core.Exceptions
{
    [Serializable]
    public class DuplicateAccountException : Exception
    {
        public DuplicateAccountException(GatewayAccount account)
            : base($"There is an account already with the name {account.Name}. Make sure to use different names for accounts.")
        {
        }
    }
}
