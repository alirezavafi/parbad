using System.Text.Json.Serialization;

namespace Persian.Plus.PaymentGateway.Gateways.AsanPardakht.Rest.Model
{
    public class PaymentTokenRequest
    {
        [JsonPropertyName("merchantConfigurationId")]
        public int MerchantConfigurationId { get; set; }

        [JsonPropertyName("serviceTypeId")]
        public int ServiceTypeId { get; set; }

        [JsonPropertyName("localInvoiceId")]
        public int LocalInvoiceId { get; set; }

        [JsonPropertyName("amountInRials")]
        public int AmountInRials { get; set; }

        [JsonPropertyName("localDate")]
        public string LocalDate { get; set; }

        [JsonPropertyName("additionalData")]
        public string AdditionalData { get; set; }

        [JsonPropertyName("callbackURL")]
        public string CallbackUrl { get; set; }

        [JsonPropertyName("paymentId")]
        public string PaymentId { get; set; }

        [JsonPropertyName("useDefaultSharing")]
        public bool UseDefaultSharing { get; set; }
    }
}