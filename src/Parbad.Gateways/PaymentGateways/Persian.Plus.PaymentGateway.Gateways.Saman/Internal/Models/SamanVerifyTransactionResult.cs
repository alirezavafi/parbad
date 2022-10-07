using System.Text.Json.Serialization;

namespace Persian.Plus.PaymentGateway.Gateways.Saman.Internal.Models
{
    public class SamanVerifyTransactionResult
    {
        [JsonPropertyName("TransactionDetail")]
        public SamanVerifyTransactionDetail SamanVerifyTransactionDetail { get; set; }

        [JsonPropertyName("PurchaseInfo")]
        public string PurchaseInfo { get; set; }

        [JsonPropertyName("ResultCode")]
        public int ResultCode { get; set; }

        [JsonPropertyName("ResultDescription")]
        public string ResultDescription { get; set; }

        [JsonPropertyName("Success")]
        public bool Success { get; set; }
    }
}