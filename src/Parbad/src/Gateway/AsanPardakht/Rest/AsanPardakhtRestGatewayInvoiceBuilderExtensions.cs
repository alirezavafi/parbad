using System;
using Parbad.InvoiceBuilder;

namespace Parbad.Gateway.AsanPardakht
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
