using Microsoft.Extensions.DependencyInjection;

namespace Persian.Plus.PaymentGateway.Core.Storage.Abstractions
{
    public interface IStorageBuilder
    {
        IServiceCollection Services { get; }
    }
}