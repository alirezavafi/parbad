// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Persian.Plus.PaymentGateway.Gateways.VirtualGateway.MiddlewareInvoker
{
    public interface IParbadVirtualGatewayMiddlewareInvoker
    {
        Task InvokeAsync();
    }
}
