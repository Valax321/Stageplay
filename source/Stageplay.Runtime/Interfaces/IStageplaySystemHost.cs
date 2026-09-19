using JetBrains.Annotations;

namespace Radish;

/// <summary>
/// Invoked every main loop tick.
/// </summary>
public delegate void MainLoopUpdateDelegate();

/// <summary>
/// Invoked when the runtime host is shutting down.
/// </summary>
public delegate void HostShutdownDelegate();

/// <summary>
/// Handles the main loop for the Stageplay runtime.
/// </summary>
/// <seealso cref="IStandaloneSystemHost"/>
[PublicAPI]
public interface IStageplaySystemHost
{
    /// <summary>
    /// Runs a single frame of the runtime.
    /// </summary>
    void RunFrame();
    
    /// <summary>
    /// Delegate invoked at the start of every game frame.
    /// </summary>
    public event MainLoopUpdateDelegate OnUpdate;
    
    /// <summary>
    /// Delegate invoked when the runtime is shutting down.
    /// </summary>
    public event HostShutdownDelegate OnShutdown;
}

/// <summary>
/// System host that owns its own main loop.
/// </summary>
[PublicAPI]
public interface IStandaloneSystemHost : IStageplaySystemHost
{
    /// <summary>
    /// Begins the main loop for the runtime.
    /// </summary>
    void Run();
}