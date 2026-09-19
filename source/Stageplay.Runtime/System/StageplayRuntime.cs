using Microsoft.Extensions.DependencyInjection;

namespace Radish;

public sealed class StageplayRuntime : IDisposable
{
    public IServiceProvider Services => _services;
    public IStageplaySystemHost Host => _host.Value;
    
    private readonly ServiceProvider _services;
    private readonly Lazy<IStageplaySystemHost> _host;
    
    internal StageplayRuntime(ServiceProvider services)
    {
        _services = services;
        _host = _services.GetRequiredService<Lazy<IStageplaySystemHost>>();
    }

    public void Dispose()
    {
        _services.Dispose();
    }
}
