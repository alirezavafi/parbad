using System;
using Persian.Plus.PaymentGateway.Core.Invoice;

namespace Persian.Plus.PaymentGateway.Gateways.VirtualGateway
{
    public static class ParbadVirtualGatewayInvoiceBuilderExtensions
    {
        /// <summary>
        /// The invoice will be sent to Persian.Plus.PaymentGateway.Core Virtual gateway.
        /// </summary>
        /// <param name="builder"></param>
        public static IInvoiceBuilder UseParbadVirtual(this IInvoiceBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.SetGateway(ParbadVirtualGateway.Name);
        }
    }
}
