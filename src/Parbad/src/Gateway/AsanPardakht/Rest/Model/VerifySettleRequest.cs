using System.Text.Json.Serialization;

namespace Parbad.Gateway.AsanPardakht.Model
{
    public class VerifySettleRequest
    {
        [JsonPropertyName("payGateTranID")]
        public string PayGateTranId { get; set; }
        [JsonPropertyName("payGateTranID")]
        public string MerchantConfigurationId { get; set; }
    }
}