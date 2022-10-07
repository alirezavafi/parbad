using System.Text.Json.Serialization;

namespace Persian.Plus.PaymentGateway.Gateways.AsanPardakht.Rest.Model
{
    public class VerifySettleRequest
    {
        [JsonPropertyName("payGateTranID")]
        public string PayGateTranId { get; set; }
        [JsonPropertyName("payGateTranID")]
        public string MerchantConfigurationId { get; set; }
    }
}