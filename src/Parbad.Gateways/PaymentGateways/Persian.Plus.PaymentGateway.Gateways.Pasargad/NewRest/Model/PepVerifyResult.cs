using System;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Persian.Plus.PaymentGateway.Gateways.Pasargad.NewRest.Model
{
    public class PepVerifyResult
    {
        [JsonProperty("invoice")]
        [JsonPropertyName("invoice")]
        public string Invoice { get; set; }

        [JsonProperty("referenceNumber")]
        [JsonPropertyName("referenceNumber")]
        public string ReferenceNumber { get; set; }

        [JsonProperty("trackId")]
        [JsonPropertyName("trackId")]
        public string TrackId { get; set; }

        [JsonProperty("maskedCardNumber")]
        [JsonPropertyName("maskedCardNumber")]
        public string MaskedCardNumber { get; set; }

        [JsonProperty("hashedCardNumber")]
        [JsonPropertyName("hashedCardNumber")]
        public string HashedCardNumber { get; set; }

        [JsonProperty("requestDate")]
        [JsonPropertyName("requestDate")]
        public DateTime RequestDate { get; set; }

        [JsonProperty("amount")]
        [JsonPropertyName("amount")]
        public long Amount { get; set; }
    }
}