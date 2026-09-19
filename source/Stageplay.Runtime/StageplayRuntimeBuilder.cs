using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Radish;

[PublicAPI]
public sealed class StageplayRuntimeBuilder
{
    public static StageplayRuntimeBuilder Create(ReadOnlySpan<string> args) => new(args);
    
    public ServiceCollection Services { get; }

    private LinkedList<Action<StageplayRuntime>> _configureCallbacks = [];

    private StageplayRuntimeBuilder(ReadOnlySpan<string> args)
    {
        Services = new ServiceCollection();
        AddDefaultServices(args);
    }

    private void AddDefaultServices(ReadOnlySpan<string> args)
    {
        Services.AddTransient(typeof(Lazy<>), typeof(ServiceLazy<>));
        Services.AddSingleton<ICommandLineArguments>(new CommandLine(args));
    }

    public StageplayRuntimeBuilder ConfigureWith(Action<StageplayRuntime> configureCallback)
    {
        _configureCallbacks.AddLast(configureCallback);
        return this;
    }

    public StageplayRuntime Build()
    {
        var serviceProvider = Services.BuildServiceProvider(true);
        var runtime = new StageplayRuntime(serviceProvider);

        foreach (var cb in _configureCallbacks)
            cb(runtime);

        return runtime;
    }
}