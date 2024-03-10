using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Persian.Plus.PaymentGateway.Gateways.Pasargad.NewRest.Model
{
    public class PepInquiryResult
    {
        [JsonProperty("status")]
        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonProperty("trackId")]
        [JsonPropertyName("trackId")]
        public string TrackId { get; set; }

        [JsonProperty("transactionId")]
        [JsonPropertyName("transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty("amount")]
        [JsonPropertyName("amount")]
        public int Amount { get; set; }

        [JsonProperty("cardNumber")]
        [JsonPropertyName("cardNumber")]
        public string CardNumber { get; set; }

        [JsonProperty("invoice")]
        [JsonPropertyName("invoice")]
        public string Invoice { get; set; }

        [JsonProperty("url")]
        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonProperty("referenceNumber")]
        [JsonPropertyName("referenceNumber")]
        public string ReferenceNumber { get; set; }

        [JsonProperty("requestDate")]
        [JsonPropertyName("requestDate")]
        public string RequestDate { get; set; }
    }
}