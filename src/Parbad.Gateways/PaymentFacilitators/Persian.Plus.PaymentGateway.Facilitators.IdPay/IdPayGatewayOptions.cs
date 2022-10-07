namespace Persian.Plus.PaymentGateway.Facilitators.IdPay
{
    public class IdPayGatewayOptions
    {
        public string ApiRequestUrl { get; set; } = "https://api.idpay.ir/v1.1/payment";

        public string ApiVerificationUrl { get; set; } = "https://api.idpay.ir/v1.1/payment/verify";
    }
}
