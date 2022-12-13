using System;
using Persian.Plus.PaymentGateway.Core.Invoice;

namespace Persian.Plus.PaymentGateway.Gateways.Pasargad.Rest
{
    public static class PasargadRestGatewayInvoiceBuilderExtensions
    {
        /// <summary>
        /// The invoice will be sent to Asan Pardakht gateway.
        /// </summary>
        /// <param name="builder"></param>
        public static IInvoiceBuilder UsePasargadRest(this IInvoiceBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.SetGateway(PasargadRestGateway.Name);
        }
    }
}
