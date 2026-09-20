using Foster.Framework;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Radish.Foster.Content;

namespace Radish.Foster;

/// <summary>
/// Runtime host for Stageplay's Foster implementation.
/// </summary>
[PublicAPI]
public class StageplayApp : App, IStandaloneSystemHost
{
    /// <summary>
    /// The service provider for the runtime.
    /// </summary>
    public IServiceProvider Services { get; }
    
    /// <summary>
    /// The content manager for the app.
    /// </summary>
    public ContentManager Content { get; }
    
    /// <summary>
    /// The time provider for the runtime.
    /// </summary>
    public ITimeProvider TimeProvider { get; }
    
    /// <summary>
    /// The audio provider for the runtime.
    /// </summary>
    /// <remarks>Foster does not have an audio implementation, so a dummy provider is used.</remarks>
    public IAudioProvider AudioProvider { get; }
    
    /// <summary>
    /// The resource provider for the runtime.
    /// </summary>
    public IResourcesProvider Resources { get; }
    
    /// <inheritdoc/>
    public event HostStartupDelegate? OnStartup;
    
    /// <inheritdoc/>
    public event HostUpdateDelegate? OnUpdate;
    
    /// <inheritdoc/>
    public event HostShutdownDelegate? OnShutdown;
    
    /// <summary>
    /// Creates a new app instance. Do not call this directly, it needs to be public for dependency injection to be able to create it.
    /// </summary>
    public StageplayApp(IServiceProvider services) : base(MakeAppConfigFromServices(services))
    {
        Services = services;
        Content = new ContentManager(this);
        Content.TitleStorageReady += ActualStartup;

        Resources = new FosterResourcesProvider(this);
        TimeProvider = new FosterTimeProvider(this);
        // Foster doesn't have an audio system, just do nothing for now
        AudioProvider = new NullAudioProvider();
    }

    /// <inheritdoc/>
    protected override void Startup()
    {
        Content.LoadTitleStorage();
    }
    
    private void ActualStartup()
    {
        MountContent();
        OnStartup?.Invoke();
    }
    
    /// <summary>
    /// Mounts additional content paths.
    /// </summary>
    protected virtual void MountContent()
    {}

    /// <inheritdoc/>
    protected override void Shutdown()
    {
        OnShutdown?.Invoke();
    }

    /// <inheritdoc/>
    protected override void Update()
    {
        if (!Content.IsTitleStorageReady)
            return;
        
        OnUpdate?.Invoke();
    }

    /// <inheritdoc/>
    protected override void Render()
    {
        if (!Content.IsTitleStorageReady)
        {
            Window.Clear(Color.Black);
            return;
        }
        
        Window.Clear(Color.CornflowerBlue);
    }

    /// <inheritdoc/>
    public void RunFrame()
    {
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
