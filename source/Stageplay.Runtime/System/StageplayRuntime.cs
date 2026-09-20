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

    /// <summary>
    /// Offers interfaces for loading various types of game assets.
    /// </summary>
    public IStageplayResources Resources => _resources.Value;

    /// <summary>
    /// The global Lua VM for the runtime.
    /// </summary>
    public ILuaVm Lua => _lua.Value;
    
    private readonly ServiceProvider _services;
    private readonly Lazy<IStageplaySystemHost> _host;
    private readonly Lazy<IStageplayResources> _resources;
    private readonly Lazy<ILuaVm> _lua;
    
    internal StageplayRuntime(ServiceProvider services)
    {
        _services = services;
        _host = _services.GetRequiredService<Lazy<IStageplaySystemHost>>();
        _resources = _services.GetRequiredService<Lazy<IStageplayResources>>();
        _lua = _services.GetRequiredService<Lazy<ILuaVm>>();
        
        Host.OnStartup += Startup;
        Host.OnUpdate += Update;
        Host.OnShutdown += Shutdown;
    }

    private void Startup()
    {
        Log.Info("Runtime startup");

        if (Lua.LoadAndExecuteModule("main"))
        {
            Log.Info("Loaded lua main module");
            var mainFunc = Lua.FindGlobalFunction("main");
            if (mainFunc is not null)
                Lua.CallSync(mainFunc);
        }
    }
    
    private void Update()
    {
        
    }
    
    private void Shutdown()
    {
        Log.Info("Runtime shutdown");
    }

    /// <summary>
    /// Disposes the runtime's resources.
    /// </summary>
    public void Dispose()
    {
        _services.Dispose();
    }
}
