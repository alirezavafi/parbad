// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Persian.Plus.PaymentGateway.Core.Builder;

namespace Persian.Plus.PaymentGateway.Core.Options
{
    public static class ParbadOptionsExtensions
    {
        /// <summary>
        /// Configures the Persian.Plus.PaymentGateway.Core options.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="setupOptions"></param>
        public static IParbadBuilder ConfigureOptions(this IParbadBuilder builder, Action<ParbadOptions> setupOptions)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (setupOptions == null) throw new ArgumentNullException(nameof(setupOptions));

            builder.Services.Configure(setupOptions);

            RegisterMessagesOptions(builder);

            return builder;
        }

        private static void RegisterMessagesOptions(IParbadBuilder builder)
        {
            builder.Services.AddTransient<IOptions<MessagesOptions>>(provider =>
            {
                var messages = provider.GetRequiredService<IOptions<ParbadOptions>>().Value.Messages;

                return new OptionsWrapper<MessagesOptions>(messages);
            });
        }
    }
}
