﻿namespace Persian.Plus.PaymentGateway.Gateways.Pasargad.Soap
{
    public class PasargadSoapGatewayOptions
    {
        public string PaymentPageUrl { get; set; } = "https://pep.shaparak.ir/gateway.aspx";

        public string ApiCheckPaymentUrl { get; set; } = "https://pep.shaparak.ir/CheckTransactionResult.aspx";

        public string ApiVerificationUrl { get; set; } = "https://pep.shaparak.ir/VerifyPayment.aspx";

        public string ApiRefundUrl { get; set; } = "https://pep.shaparak.ir/DoRefund.aspx";
    }
}
