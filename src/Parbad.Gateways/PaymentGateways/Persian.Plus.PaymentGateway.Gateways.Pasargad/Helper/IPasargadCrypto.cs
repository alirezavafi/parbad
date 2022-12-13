namespace Persian.Plus.PaymentGateway.Gateways.Pasargad.Helper
{
    public interface IPasargadCrypto
    {
        string Encrypt(string privateKey, string data);
    }
}
