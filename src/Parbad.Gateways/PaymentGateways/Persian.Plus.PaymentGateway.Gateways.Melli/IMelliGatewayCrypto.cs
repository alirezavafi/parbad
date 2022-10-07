namespace Persian.Plus.PaymentGateway.Gateways.Melli
{
    public interface IMelliGatewayCrypto
    {
        string Encrypt(string terminalKey, string data);
    }
}
