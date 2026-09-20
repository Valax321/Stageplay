using Foster.Framework;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Radish.Foster.Content;

namespace Radish.Foster;

[PublicAPI]
public class StageplayApp : App, IStandaloneSystemHost
{
    public IServiceProvider Services { get; }
    public ContentManager Content { get; }
    public ITimeProvider TimeProvider { get; }
    public IAudioProvider AudioProvider { get; }
    public IResourcesProvider Resources { get; }
    
    public event HostStartupDelegate? OnStartup;
    public event HostUpdateDelegate? OnUpdate;
    public event HostShutdownDelegate? OnShutdown;
    
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

    protected override void Startup()
    {
        Content.LoadTitleStorage();
    }
    
    private void ActualStartup()
    {
        MountContent();
        OnStartup?.Invoke();
    }
    
    protected virtual void MountContent()
    {}

    protected override void Shutdown()
    {
        OnShutdown?.Invoke();
    }

    protected override void Update()
    {
        if (!Content.IsTitleStorageReady)
            return;
        
        OnUpdate?.Invoke();
    }

    protected override void Render()
    {
        if (!Content.IsTitleStorageReady)
        {
            Window.Clear(Color.Black);
            return;
        }
        
        Window.Clear(Color.CornflowerBlue);
    }

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
