using Microsoft.Extensions.DependencyInjection;

namespace Radish;

internal sealed class ServiceLazy<T>(IServiceProvider services) : Lazy<T>(() => Factory(services)) 
    where T : notnull
{
    private static T Factory(IServiceProvider services) 
        => services.GetRequiredService<T>();
}