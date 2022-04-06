namespace Parbad.Gateway.AsanPardakht
{
    public interface IAsanPardakhtSoapCrypto
    {
        string Encrypt(string input, string key, string iv);

        string Decrypt(string input, string key, string iv);
    }
}
