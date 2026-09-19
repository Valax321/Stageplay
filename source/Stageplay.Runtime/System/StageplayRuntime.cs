using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Radish;

/// <summary>
/// The entry point for a Stageplay application.
/// </summary>
[PublicAPI]
public sealed class StageplayRuntime : IDisposable
{
    /// <summary>
    /// The global services owned by this runtime.
    /// </summary>
    public IServiceProvider Services => _services;
    
    /// <summary>
    /// Hosts the main loop for the runtime instance.
    /// Offers callbacks about the current main loop state.
    /// </summary>
    public IStageplaySystemHost Host => _host.Value;
    
    private readonly ServiceProvider _services;
    private readonly Lazy<IStageplaySystemHost> _host;
    
    internal StageplayRuntime(ServiceProvider services)
    {
        _services = services;
        _host = _services.GetRequiredService<Lazy<IStageplaySystemHost>>();
    }

    /// <summary>
    /// Disposes the runtime's resources.
    /// </summary>
    public void Dispose()
    {
        _services.Dispose();
    }
}
