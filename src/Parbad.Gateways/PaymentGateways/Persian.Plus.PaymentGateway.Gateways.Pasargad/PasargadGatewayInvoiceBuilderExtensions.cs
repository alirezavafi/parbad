using System;
using Persian.Plus.PaymentGateway.Core.Invoice;

namespace Persian.Plus.PaymentGateway.Gateways.Pasargad
{
    public static class PasargadGatewayInvoiceBuilderExtensions
    {
        /// <summary>
        /// The invoice will be sent to Pasargad gateway.
        /// </summary>
        /// <param name="builder"></param>
        public static IInvoiceBuilder UsePasargad(this IInvoiceBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.SetGateway(PasargadGateway.Name);
        }
    }
}
