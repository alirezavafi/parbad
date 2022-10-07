using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Persian.Plus.PaymentGateway.Core.Utilities;

namespace Persian.Plus.PaymentGateway.Core.Storage.Abstractions
{
    public static class StorageBuilderExtensions
    {
        /// <summary>
        /// Adds an implementation of <see cref="IPaymentStorage"/> which required by Persian.Plus.PaymentGateway.Core for managing the storage operations.
        /// </summary>
        /// <typeparam name="TManager"></typeparam>
        /// <param name="builder"></param>
        /// <param name="lifetime">The lifetime of given StorageManager.</param>
        public static IStorageBuilder AddStorage<TManager>(this IStorageBuilder builder, ServiceLifetime lifetime) where TManager : class, IPaymentStorage
        {
            builder.Services.AddOrUpdate<IPaymentStorage, TManager>(lifetime);

            return builder;
        }

        /// <summary>
        /// Adds an implementation of <see cref="IPaymentStorage"/> which required by Persian.Plus.PaymentGateway.Core for managing the storage operations.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="paymentStorage"></param>
        public static IStorageBuilder AddStorage(this IStorageBuilder builder, IPaymentStorage paymentStorage)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (paymentStorage == null) throw new ArgumentNullException(nameof(paymentStorage));

            builder.Services
                .RemoveAll<IPaymentStorage>()
                .AddSingleton(paymentStorage);

            return builder;
        }

        /// <summary>
        /// Adds an implementation of <see cref="IPaymentStorage"/> which required by Persian.Plus.PaymentGateway.Core for managing the storage operations.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="factory"></param>
        /// <param name="lifetime">The lifetime of given StorageManager.</param>
        public static IStorageBuilder AddStorage(this IStorageBuilder builder, Func<IServiceProvider, IPaymentStorage> factory, ServiceLifetime lifetime)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            builder.Services.AddOrUpdate(factory, lifetime);

            return builder;
        }
    }
}
