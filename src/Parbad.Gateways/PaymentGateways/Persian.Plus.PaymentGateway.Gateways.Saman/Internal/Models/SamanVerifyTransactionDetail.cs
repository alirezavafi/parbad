using System.Text.Json.Serialization;

namespace Persian.Plus.PaymentGateway.Gateways.Saman.Internal.Models
{
    public class SamanVerifyTransactionDetail
    {
        [JsonPropertyName("RRN")]
        public string Rrn { get; set; }

        [JsonPropertyName("RefNum")]
        public object ReferenceNumber { get; set; }

        [JsonPropertyName("MaskedPan")]
        public object MaskedPan { get; set; }

        [JsonPropertyName("HashedPan")]
        public object HashedPan { get; set; }

        [JsonPropertyName("TerminalNumber")]
        public int TerminalNumber { get; set; }

        [JsonPropertyName("OrginalAmount")]
        public int OrginalAmount { get; set; }

        [JsonPropertyName("AffectiveAmount")]
        public int AffectiveAmount { get; set; }

        [JsonPropertyName("StraceDate")]
        public object StraceDate { get; set; }

        [JsonPropertyName("StraceNo")]
        public object StraceNo { get; set; }
    }
}