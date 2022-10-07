using System;

namespace Persian.Plus.PaymentGateway.Gateways.AsanPardakht.Soap.Internal
{
    internal class AsanPardakhtSoapCrypto : IAsanPardakhtSoapCrypto
    {
        public string Encrypt(string input, string key, string iv)
        {
            var keyBytes = Convert.FromBase64String(key);
            var ivBytes = Convert.FromBase64String(iv);
            return CipherHelper.Encrypt(input, keyBytes, ivBytes);
        }

        public string Decrypt(string input, string key, string iv)
        {
            var keyBytes = Convert.FromBase64String(key);
            var ivBytes = Convert.FromBase64String(iv);
            return CipherHelper.Decrypt(input, keyBytes, ivBytes);
        }
    }
}