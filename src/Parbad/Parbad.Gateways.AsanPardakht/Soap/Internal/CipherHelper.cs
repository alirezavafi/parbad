using System;
using System.Linq;
using System.Text;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;

namespace Parbad.Gateway.AsanPardakht.Internal
{
    public static class CipherHelper
    {
        public static string Encrypt(string plainText, byte[] key, byte[] iv)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            var engine = new RijndaelEngine(256);
            var blockCipher = new CbcBlockCipher(engine);
            var cipher = new PaddedBufferedBlockCipher(blockCipher, new Pkcs7Padding());
            var keyParam = new KeyParameter(key);
            var keyParamWithIV = new ParametersWithIV(keyParam, iv, 0, 32);

            cipher.Init(true, keyParamWithIV);
            var comparisonBytes = new byte[cipher.GetOutputSize(plainTextBytes.Length)];
            var length = cipher.ProcessBytes(plainTextBytes, comparisonBytes, 0);

            cipher.DoFinal(comparisonBytes, length);
            return Convert.ToBase64String(comparisonBytes);
        }

        public static string Decrypt(string cipherText, byte[] key, byte[] iv)
        {
            var cipherTextBytes = Convert.FromBase64String(cipherText);
            var engine = new RijndaelEngine(256);
            var blockCipher = new CbcBlockCipher(engine);
            var cipher = new PaddedBufferedBlockCipher(blockCipher, new Pkcs7Padding());
            var keyParam = new KeyParameter(key);
            var keyParamWithIV = new ParametersWithIV(keyParam, iv, 0, 32);

            cipher.Init(false, keyParamWithIV);
            var comparisonBytes = new byte[cipher.GetOutputSize(cipherTextBytes.Length)];
            var length = cipher.ProcessBytes(cipherTextBytes, comparisonBytes, 0);

            cipher.DoFinal(comparisonBytes, length);
            //return Convert.ToBase64String(saltStringBytes.Concat(ivStringBytes).Concat(comparisonBytes).ToArray());

            var nullIndex = comparisonBytes.Length - 1;
            while (comparisonBytes[nullIndex] == (byte) 0)
                nullIndex--;
            comparisonBytes = comparisonBytes.Take(nullIndex + 1).ToArray();


            var result = Encoding.UTF8.GetString(comparisonBytes, 0, comparisonBytes.Length);

            return result;
        }
    }
}