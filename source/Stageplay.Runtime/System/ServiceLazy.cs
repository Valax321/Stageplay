using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace Radish;

internal sealed class ServiceLazy<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(
    IServiceProvider services) : Lazy<T>(() => Factory(services))
    where T : notnull
{
    private static T Factory(IServiceProvider services)
        => services.GetRequiredService<T>();
}