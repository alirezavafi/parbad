// Copyright (c) Parbad.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad
{
    /// <summary>
    /// Describes the status of the Verify operation.
    /// </summary>
    public enum PaymentVerifyResultStatus
    {
        /// <summary>
        /// The Verify operation was successful.
        /// </summary>
        Succeed,

        /// <summary>
        /// The Verify operation is failed.
        /// </summary>
        Failed,

        /// <summary>
        /// The invoice has been verified before.
        /// </summary>
        AlreadyVerified
    }
}
