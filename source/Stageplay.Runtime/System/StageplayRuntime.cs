using Foster.Framework;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Radish.Content;
using Radish.Impl;

namespace Radish;

/// <summary>
/// Runtime host for Stageplay's Foster implementation.
/// </summary>
[PublicAPI]
internal sealed class StageplayRuntime : App, IStageplayRuntime
{
    /// <summary>
    /// The service provider for the runtime.
    /// </summary>
    public IServiceProvider Services { get; }

    public new GraphicsDevice GraphicsDevice => base.GraphicsDevice;

    private ContentManager _content;
    private Game _game;
    private LifecycleHooksImpl _lifecycleHooks;
    
    /// <summary>
    /// Creates a new app instance. Do not call this directly, it needs to be public for dependency injection to be able to create it.
    /// </summary>
    public StageplayRuntime(IServiceProvider services) : base(MakeAppConfigFromServices(services))
    {
        Services = services;

        _game = Services.GetRequiredService<Game>();
        _content = (ContentManager)Services.GetRequiredService<IContentManager>();
        _lifecycleHooks = (LifecycleHooksImpl)Services.GetRequiredService<IRuntimeLifecycleHooks>();
    }

    /// <inheritdoc/>
    protected override void Startup()
    {
        _content.TitleStorageReady += ActualStartup;
        _content.LoadTitleStorage();
    }
    
    private void ActualStartup()
    {
        _lifecycleHooks.RunStartupHooks();
        _game.MountContent(_content);
        _game.PostRuntimeInit();
    }

    /// <inheritdoc/>
    protected override void Shutdown()
    {
        _lifecycleHooks.RunShutdownHooks();
    }

    /// <inheritdoc/>
    protected override void Update()
    {
        if (!_content.IsTitleStorageReady)
            return;
        
        _lifecycleHooks.RunUpdateHooks();
    }

    /// <inheritdoc/>
    protected override void Render()
    {
        if (!_content.IsTitleStorageReady)
        {
            Window.Clear(Color.Black);
            return;
        }
        
        Window.Clear(Color.CornflowerBlue);
    }
    
    private static AppConfig MakeAppConfigFromServices(IServiceProvider services)
    {
        var gameInfo = services.GetRequiredService<GameInfo>();
        var cmdLine = services.GetRequiredService<ICommandLineArguments>();

        var sz = gameInfo.DesignSize;
        var fullscreen = true;

        if (cmdLine.TryGetValue("w", out var w) && int.TryParse(w, out var ww))
            sz.Width = ww;

        if (cmdLine.TryGetValue("h", out var h) && int.TryParse(h, out var hh))
            sz.Height = hh;

        if (cmdLine.ContainsKey("window") || cmdLine.ContainsKey("windowed"))
            fullscreen = false;

        var flags = AppFlags.NoHeaderLog;
        if (cmdLine.ContainsKey("gpuDebug"))
            flags |= AppFlags.GraphicsDebugging;
        
        return new AppConfig(gameInfo.ApplicationName, gameInfo.ApplicationName, sz.Width, sz.Height, fullscreen, false,
            UpdateMode.UnlockedStep(), Flags: flags);
    }
}
