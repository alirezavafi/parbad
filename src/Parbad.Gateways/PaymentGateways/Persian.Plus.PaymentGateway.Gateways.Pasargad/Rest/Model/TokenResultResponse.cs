using System.Text.Json.Serialization;

namespace Persian.Plus.PaymentGateway.Gateways.Pasargad.Rest.Model
{
    public class TokenResultResponse
    {
        [JsonPropertyName("IsSuccess")]
        public bool IsSuccess { get; set; }
        [JsonPropertyName("Message")]
        public string Message { get; set; }
        [JsonPropertyName("Token")]
        public string Token { get; set; }
    }
}