using System.Text.Json.Serialization;

namespace Persian.Plus.PaymentGateway.Gateways.Pasargad.Rest.Model
{
    public class TransactionResultResponse
    {
        [JsonPropertyName("TraceNumber")]
        public long TraceNumber { get; set; }
        [JsonPropertyName("ReferenceNumber")]
        public long ReferenceNumber { get; set; }
        [JsonPropertyName("TransactionDate")]
        public string TransactionDate { get; set; }
        [JsonPropertyName("Action")]
        public string Action { get; set; }
        [JsonPropertyName("TransactionReferenceID")]
        public string TransactionReferenceId { get; set; }
        [JsonPropertyName("InvoiceNumber")]
        public string InvoiceNumber { get; set; }
        [JsonPropertyName("InvoiceDate")]
        public string InvoiceDate { get; set; }
        [JsonPropertyName("MerchantCode")]
        public int MerchantCode { get; set; }
        [JsonPropertyName("TerminalCode")]
        public int TerminalCode { get; set; }
        [JsonPropertyName("Amount")]
        public decimal Amount { get; set; }
        [JsonPropertyName("IsSuccess")]
        public bool IsSuccess { get; set; }
        [JsonPropertyName("Message")]
        public string Message { get; set; }
    }
}