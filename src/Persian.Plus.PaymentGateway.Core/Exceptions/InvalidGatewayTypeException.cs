// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;

namespace Persian.Plus.PaymentGateway.Core.Exceptions
{
    [Serializable]
    public class InvalidGatewayTypeException : Exception
    {
        public InvalidGatewayTypeException(Type type) : base($"The type {type} is not a valid gateway type.")
        {
        }
    }
}
