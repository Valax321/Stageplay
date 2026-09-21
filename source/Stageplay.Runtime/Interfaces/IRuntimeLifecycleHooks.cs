namespace Radish;

/// <summary>
/// Invoked when the host has finished initialising its internals, and Stageplay can now start initialising itself.
/// </summary>
public delegate void RuntimeStartupDelegate();

/// <summary>
/// Invoked every main loop tick.
/// </summary>
public delegate void RuntimeUpdateDelegate();

/// <summary>
/// Invoked when the runtime host is shutting down.
/// </summary>
public delegate void RuntimeShutdownDelegate();

public interface IRuntimeLifecycleHooks
{
    event RuntimeStartupDelegate OnStartup;
    event RuntimeUpdateDelegate OnUpdate;
    event RuntimeShutdownDelegate OnShutdown;
}