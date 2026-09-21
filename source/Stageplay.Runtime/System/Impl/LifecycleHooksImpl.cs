namespace Radish.Impl;

internal sealed class LifecycleHooksImpl : IRuntimeLifecycleHooks
{
    public event RuntimeStartupDelegate? OnStartup;
    public event RuntimeUpdateDelegate? OnUpdate;
    public event RuntimeShutdownDelegate? OnShutdown;

    public void RunStartupHooks() => OnStartup?.Invoke();
    public void RunUpdateHooks() => OnUpdate?.Invoke();
    public void RunShutdownHooks() => OnShutdown?.Invoke();
}