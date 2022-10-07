namespace Persian.Plus.PaymentGateway.Gateways.AsanPardakht.Soap
{
    public class AsanPardakhtSoapGatewayOptions
    {
        public string PaymentPageUrl { get; set; } = "https://asan.shaparak.ir/";

        public string ApiUrl { get; set; } = "https://ipgsoap.asanpardakht.ir/paygate/merchantservices.asmx";
    }
}
