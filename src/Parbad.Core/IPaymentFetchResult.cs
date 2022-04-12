// Copyright (c) Parbad.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad
{
    /// <summary>
    /// Describes the status of the fetched invoice.
    /// </summary>
    public enum PaymentFetchResultStatus
    {
        /// <summary>
        /// The invoice is ready for verifying.
        /// </summary>
        ReadyForVerifying,

        /// <summary>
        /// The invoice is already processed before.
        /// </summary>
        AlreadyProcessed,

        /// <summary>
        /// Payment is failed.
        /// </summary>
        Failed
    }
}
