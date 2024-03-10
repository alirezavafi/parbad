using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Persian.Plus.PaymentGateway.Gateways.Pasargad.NewRest.Model
{
    public class PepPurchaseResult
    {
        [JsonProperty("urlId")]
        [JsonPropertyName("urlId")]
        public string UrlId { get; set; }
        [JsonProperty("url")]
        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}