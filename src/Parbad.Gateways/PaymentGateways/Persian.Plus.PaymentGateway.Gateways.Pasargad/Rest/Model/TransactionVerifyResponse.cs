using System.Text.Json.Serialization;

namespace Persian.Plus.PaymentGateway.Gateways.Pasargad.Rest.Model
{
    public class TransactionVerifyResponse
    {
        [JsonPropertyName("IsSuccess")]
        public bool IsSuccess { get; set; }
        [JsonPropertyName("Message")]
        public string Message { get; set; }
        [JsonPropertyName("MaskedCardNumber")]
        public string MaskedCardNumber { get; set; }
        [JsonPropertyName("HashCardNumber")]
        public string HashCardNumber { get; set; }
        [JsonPropertyName("ShaparakRefNumber")]
        public string ShaparakRefNumber { get; set; }
    }
}