// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Persian.Plus.PaymentGateway.Core.Properties;

namespace Persian.Plus.PaymentGateway.Core.Exceptions
{
    [Serializable]
    public class ParbadServiceNotInitializedException : Exception
    {
        public ParbadServiceNotInitializedException() : base(Resources.CannotGetParbadServiceException)
        {
        }
    }
}
