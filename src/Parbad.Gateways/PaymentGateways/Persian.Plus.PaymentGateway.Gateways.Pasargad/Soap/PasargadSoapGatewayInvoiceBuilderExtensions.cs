using System;
using Persian.Plus.PaymentGateway.Core.Invoice;

namespace Persian.Plus.PaymentGateway.Gateways.Pasargad
{
    public static class PasargadSoapGatewayInvoiceBuilderExtensions
    {
        /// <summary>
        /// The invoice will be sent to Pasargad gateway.
        /// </summary>
        /// <param name="builder"></param>
        public static IInvoiceBuilder UsePasargadSoap(this IInvoiceBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.SetGateway(PasargadSoapGateway.Name);
        }
    }
}
