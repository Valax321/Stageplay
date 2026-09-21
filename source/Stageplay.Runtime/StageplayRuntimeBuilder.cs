using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Radish.Lua;

namespace Radish;

/// <summary>
/// Configuration builder object for setting up runtime configuration.
/// </summary>
[PublicAPI]
public sealed class StageplayRuntimeBuilder
{
    /// <summary>
    /// Creates a new runtime configuration builder.
    /// </summary>
    /// <param name="args">The program's command line arguments.</param>
    /// <returns>A new runtime builder.</returns>
    public static StageplayRuntimeBuilder Create(ReadOnlySpan<string> args) => new(args);
    
    /// <summary>
    /// The service collection that this builder is building.
    /// </summary>
    public ServiceCollection Services { get; }

    private LinkedList<Action<IStageplayRuntime>> _configureCallbacks = [];

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

    private void AddFallbackServices()
    {
        Services.TryAddSingleton<ILogSink>(new ConsoleLogSink());
        Services.TryAddSingleton<ILuaVm, LuaVm>();
    }
    
    /// <summary>
    /// Adds a configure callback to the builder.
    /// The callback will be invoked just after the runtime is created.
    /// </summary>
    /// <param name="configureCallback">Method to do post-build configure work with.</param>
    /// <returns>Same builder instance, for fluent API calls.</returns>
    public StageplayRuntimeBuilder WithConfigure(Action<IStageplayRuntime> configureCallback)
    {
        _configureCallbacks.AddLast(configureCallback);
        return this;
    }

    private static void InstallLogSink(IServiceProvider services)
    {
        var logSink = services.GetService<ILogSink>();
        if (logSink is not null)
            Log.SetSink(logSink);
    }

    /// <summary>
    /// Creates the actual runtime from the configuration state.
    /// </summary>
    /// <returns>A new runtime instance.</returns>
    public IStageplayRuntime Build()
    {
        AddFallbackServices();
        
        var serviceProvider = Services.BuildServiceProvider(true);
        InstallLogSink(serviceProvider);
        
        Log.Info($"Stageplay {GitVersionInformation.SemVer}.{GitVersionInformation.ShortSha}");
        Log.Info($"Framework: {RuntimeInformation.FrameworkDescription}");
        Log.Info($"Platform: {RuntimeInformation.OSDescription} {RuntimeInformation.ProcessArchitecture}");

        var runtime = serviceProvider.GetRequiredService<IStageplayRuntime>();

        foreach (var cb in _configureCallbacks)
            cb(runtime);

        return runtime;
    }
}