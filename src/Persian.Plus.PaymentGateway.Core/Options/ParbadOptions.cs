﻿namespace Persian.Plus.PaymentGateway.Core.Options
{
    /// <summary>
    /// Provides configuration for Persian.Plus.PaymentGateway.Core.
    /// </summary>
    public class ParbadOptions
    {
        /// <summary>
        /// Enables or disables the logging. The default value is true.
        /// </summary>
        public bool EnableLogging { get; set; } = true;

        /// <summary>
        /// Contains all messages that Persian.Plus.PaymentGateway.Core uses in results.
        /// </summary>
        public MessagesOptions Messages { get; set; } = new MessagesOptions();
    }
}
