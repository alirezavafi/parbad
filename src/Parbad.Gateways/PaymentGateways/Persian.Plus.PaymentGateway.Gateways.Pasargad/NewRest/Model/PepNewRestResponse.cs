using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Persian.Plus.PaymentGateway.Gateways.Pasargad.NewRest.Model
{
    public class PepNewRestResponse<T>
    {
        [JsonProperty("resultMsg")]
        [JsonPropertyName("resultMsg")]
        public string ResultMsg { get; set; }

        [JsonProperty("resultCode")]
        [JsonPropertyName("resultCode")]
        public int ResultCode { get; set; }

        [JsonProperty("data")]
        [JsonPropertyName("data")]
        public T Data { get; set; }
    }
}