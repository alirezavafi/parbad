using System;
using Parbad.InvoiceBuilder;

namespace Parbad.Gateway.AsanPardakht
{
    public static class AsanPardakhtSoapGatewayInvoiceBuilderExtensions
    {
        /// <summary>
        /// The invoice will be sent to Asan Pardakht gateway.
        /// </summary>
        /// <param name="builder"></param>
        public static IInvoiceBuilder UseAsanPardakhtSoap(this IInvoiceBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.SetGateway(AsanPardakhtSoapGateway.Name);
        }
    }
}
