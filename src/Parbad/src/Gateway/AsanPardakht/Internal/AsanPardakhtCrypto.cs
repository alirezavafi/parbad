using System;
using System.IO;
using System.Security.Cryptography;

namespace Parbad.Gateway.AsanPardakht.Internal
{
    internal class AsanPardakhtCrypto : IAsanPardakhtCrypto
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