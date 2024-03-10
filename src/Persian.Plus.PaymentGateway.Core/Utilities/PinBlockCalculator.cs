using System;
using System.Security.Cryptography;

namespace Persian.Plus.PaymentGateway.Core.Utilities
{
    public class PinBlockCalculator 
    {
        public string GetPanPinBlock(string pan, string keyHex, string vectorHex)
        {
            if (string.IsNullOrWhiteSpace(pan)) throw new ArgumentNullException(nameof(pan));
            if (string.IsNullOrWhiteSpace(keyHex)) throw new ArgumentNullException(nameof(keyHex));
            if (string.IsNullOrWhiteSpace(vectorHex)) throw new ArgumentNullException(nameof(vectorHex));
            
            var key = Convert.FromHexString(keyHex);
            var iv = Convert.FromHexString(vectorHex);
            var panBytes = Convert.FromHexString(pan); 
            var tdes = new DESCryptoServiceProvider();
            tdes.Key = key;
            tdes.IV = iv;
            tdes.Mode = CipherMode.CBC;
            tdes.Padding = PaddingMode.None;
            ICryptoTransform cTransform = tdes.CreateEncryptor();
            //byte[] result = EncryptTextToMemory(data, cTransform);
            byte[] result = cTransform.TransformFinalBlock(panBytes, 0, panBytes.Length);
            var encryptedPan = Convert.ToHexString(result);
            return encryptedPan;
        }
    }
}