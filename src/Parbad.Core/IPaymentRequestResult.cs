// Copyright (c) Parbad.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;

namespace Parbad
{
    /// <summary>
    /// Describes the status of the requested invoice.
    /// </summary>
    public enum PaymentRequestResultStatus
    {
        /// <summary>
        /// Request was successful.
        /// </summary>
        Succeed,

        /// <summary>
        /// Request is failed.
        /// </summary>
        Failed,

        /// <summary>
        /// The tracking number is already exists or used before.
        /// </summary>
        TrackingNumberAlreadyExists
    }
}
