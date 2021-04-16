using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Parbad.Storage.Abstractions
{
    public static class StorageBuilderExtensions
    {
        /// <summary>
        /// Adds an implementation of <see cref="IStorage"/> which required by Parbad for managing the storage operations.
        /// </summary>
        /// <typeparam name="TManager"></typeparam>
        /// <param name="builder"></param>
        /// <param name="lifetime">The lifetime of given StorageManager.</param>
        public static IStorageBuilder AddStorage<TManager>(this IStorageBuilder builder, ServiceLifetime lifetime) where TManager : class, IStorage
        {
            builder.Services.AddOrUpdate<IStorage, TManager>(lifetime);

            return builder;
        }

        /// <summary>
        /// Adds an implementation of <see cref="IStorage"/> which required by Parbad for managing the storage operations.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="storage"></param>
        public static IStorageBuilder AddStorage(this IStorageBuilder builder, IStorage storage)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (storage == null) throw new ArgumentNullException(nameof(storage));

            builder.Services
                .RemoveAll<IStorage>()
                .AddSingleton(storage);

            return builder;
        }

        /// <summary>
        /// Adds an implementation of <see cref="IStorage"/> which required by Parbad for managing the storage operations.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="factory"></param>
        /// <param name="lifetime">The lifetime of given StorageManager.</param>
        public static IStorageBuilder AddStorage(this IStorageBuilder builder, Func<IServiceProvider, IStorage> factory, ServiceLifetime lifetime)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            builder.Services.AddOrUpdate(factory, lifetime);

            return builder;
        }
    }
}
