// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;

namespace Persian.Plus.PaymentGateway.Gateways.Melli
{
    /// <summary>
    /// The exception that is thrown when an error by signing is occurred.
    /// </summary>
    [Serializable]
    public class MelliGatewayDataSigningException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the Persian.Plus.PaymentGateway.Core.Providers.Melli.MelliGatewayDataSigningException class.
        /// </summary>
        /// <param name="mainException">The main Exception object.</param>
        public MelliGatewayDataSigningException(Exception mainException) : base($"MelliBank signing data failed. {mainException.Message}", mainException)
        {
        }
    }
}
