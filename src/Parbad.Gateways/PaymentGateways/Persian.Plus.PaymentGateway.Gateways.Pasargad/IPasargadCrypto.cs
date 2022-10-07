namespace Persian.Plus.PaymentGateway.Gateways.Pasargad
{
    public interface IPasargadCrypto
    {
        string Encrypt(string privateKey, string data);
    }
}
