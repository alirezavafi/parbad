using System.Collections.Generic;

namespace Persian.Plus.PaymentGateway.Gateways.Pasargad.NewRest.Model
{
    internal class PasargadNewRestCallbackResult
    {
        public bool IsSucceed { get; set; }

        public string InvoiceNumber { get; set; }
        
        public string TransactionId { get; set; }

        public PepInquiryResult CallbackInquiryResult { get; set; }

        public string Message { get; set; }
    }
}