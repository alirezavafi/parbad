using System;
using System.Text.Json.Serialization;

namespace Parbad.Gateway.AsanPardakht.Model
{
    public class TransactionResultResponse
    {
        [JsonPropertyName("cardNumber")]
        public string CardNumber { get; set; }

        [JsonPropertyName("rrn")]
        public string Rrn { get; set; }

        [JsonPropertyName("refID")]
        public string RefId { get; set; }

        [JsonPropertyName("amount")]
        public string Amount { get; set; }

        [JsonPropertyName("payGateTranID")]
        public string PayGateTranId { get; set; }

        [JsonPropertyName("salesOrderID")]
        public string SalesOrderId { get; set; }

        [JsonPropertyName("hash")]
        public string Hash { get; set; }

        [JsonPropertyName("serviceTypeId")]
        public int ServiceTypeId { get; set; }

        [JsonPropertyName("serviceStatusCode")]
        public string ServiceStatusCode { get; set; }

        [JsonPropertyName("destinationMobile")]
        public string DestinationMobile { get; set; }

        [JsonPropertyName("productId")]
        public int ProductId { get; set; }

        [JsonPropertyName("productNameFa")]
        public string ProductNameFa { get; set; }

        [JsonPropertyName("productPrice")]
        public int ProductPrice { get; set; }

        [JsonPropertyName("operatorId")]
        public int OperatorId { get; set; }

        [JsonPropertyName("operatorNameFa")]
        public string OperatorNameFa { get; set; }

        [JsonPropertyName("simTypeId")]
        public int SimTypeId { get; set; }

        [JsonPropertyName("simTypeTitleFa")]
        public string SimTypeTitleFa { get; set; }

        [JsonPropertyName("billId")]
        public string BillId { get; set; }

        [JsonPropertyName("payId")]
        public string PayId { get; set; }

        [JsonPropertyName("billOrganizationNameFa")]
        public string BillOrganizationNameFa { get; set; }

        [JsonPropertyName("payGateTranDate")]
        public DateTime PayGateTranDate { get; set; }

        [JsonPropertyName("payGateTranDateEpoch")]
        public int PayGateTranDateEpoch { get; set; }
    }
}