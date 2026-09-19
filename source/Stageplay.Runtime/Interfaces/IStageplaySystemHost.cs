using JetBrains.Annotations;

namespace Radish;

public delegate void MainLoopUpdateDelegate();

public delegate void HostShutdownDelegate();

/// <summary>
/// Handles the main loop for the Stageplay runtime.
/// </summary>
[PublicAPI]
public interface IStageplaySystemHost
{
    void Initialize();
    void RunFrame();
    
    public event MainLoopUpdateDelegate OnUpdate;
    public event HostShutdownDelegate OnShutdown;
}

/// <summary>
/// System host that owns its own main loop.
/// </summary>
[PublicAPI]
public interface IStandaloneSystemHost : IStageplaySystemHost
{
    void Run();
}