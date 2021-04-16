using Microsoft.Extensions.DependencyInjection;

namespace Parbad.Storage.Abstractions
{
    public interface IStorageBuilder
    {
        IServiceCollection Services { get; }
    }
}