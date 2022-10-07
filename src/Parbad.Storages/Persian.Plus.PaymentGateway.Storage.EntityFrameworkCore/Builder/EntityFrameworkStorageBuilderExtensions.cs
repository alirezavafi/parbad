// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Persian.Plus.PaymentGateway.Core.Storage.Abstractions;
using Persian.Plus.PaymentGateway.Storage.EntityFrameworkCore.Context;
using Persian.Plus.PaymentGateway.Storage.EntityFrameworkCore.Options;

namespace Persian.Plus.PaymentGateway.Storage.EntityFrameworkCore.Builder
{
    public static class EntityFrameworkStorageBuilderExtensions
    {
        /// <summary>
        /// Uses the EntityFramework Core as a storage for saving and loading data.
        /// <para>
        /// Note: It means Persian.Plus.PaymentGateway.Core can save and load the data in different database providers
        /// such as SQL Server, MySql, Sqlite, PostgreSQL, Oracle, InMemory, etc.
        /// For more information see: https://docs.microsoft.com/en-us/ef/core/providers/.
        /// </para>
        /// <para>Note: This database is only for internal usages such as saving and loading payment information.
        /// You don't need to think about merging and using this database with your own database.
        /// The important payment information such as Tracking Number, Transaction Code, etc. will you get from the result of
        /// all payment requests.</para>
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureEfCoreOptions">Configures the EntityFrameworkCore options for Persian.Plus.PaymentGateway.Core.</param>
        public static IStorageBuilder UseEfCore(this IStorageBuilder builder, Action<EntityFrameworkCoreOptions> configureEfCoreOptions)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (configureEfCoreOptions == null) throw new ArgumentNullException(nameof(configureEfCoreOptions));

            var options = new EntityFrameworkCoreOptions();
            configureEfCoreOptions(options);

            builder.Services.Configure(configureEfCoreOptions);

            builder.Services.AddDbContext<ParbadDataContext>(dbBuilder =>
            {
                options.ConfigureDbContext?.Invoke(dbBuilder);
            });

            builder.AddStorage<EntityFrameworkCorePaymentStorage>(ServiceLifetime.Transient);

            return builder;
        }
    }
}
