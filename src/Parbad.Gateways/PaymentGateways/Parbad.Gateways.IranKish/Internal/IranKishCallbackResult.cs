// Copyright (c) Parbad.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.IranKish.Internal
{
    internal class IranKishCallbackResult
    {
        public bool IsSucceed { get; set; }

        public string Token { get; set; }

        /// <summary>
        /// Equals to TrackingNumber in Parbad.Core system.
        /// </summary>
        public long InvoiceNumber { get; set; }

        /// <summary>
        /// Equals to TransactionCode in Parbad.Core system.
        /// </summary>
        public string ReferenceId { get; set; }

        public string Message { get; set; }
    }
}