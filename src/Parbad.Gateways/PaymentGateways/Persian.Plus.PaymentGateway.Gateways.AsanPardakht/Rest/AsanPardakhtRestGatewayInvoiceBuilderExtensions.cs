using System;
using Persian.Plus.PaymentGateway.Core.Invoice;

namespace Persian.Plus.PaymentGateway.Gateways.AsanPardakht.Rest
{
    public static class AsanPardakhtRestGatewayInvoiceBuilderExtensions
    {
        /// <summary>
        /// The invoice will be sent to Asan Pardakht gateway.
        /// </summary>
        /// <param name="builder"></param>
        public static IInvoiceBuilder UseAsanPardakhtRest(this IInvoiceBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.SetGateway(AsanPardakhtRestGateway.Name);
        }
    }
}
