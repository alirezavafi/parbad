using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Persian.Plus.PaymentGateway.Gateways.Pasargad.NewRest.Model
{
    public class Role
    {
        [JsonProperty("authority")]
        [JsonPropertyName("authority")]
        public string Authority { get; set; }
    }
}