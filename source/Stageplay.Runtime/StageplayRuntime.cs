using System.Runtime.InteropServices;
using Foster.Framework;
using JetBrains.Annotations;
using Radish.Content;
using Radish.IO;
using Radish.Lua;
using Radish.Platform;

namespace Radish;

/// <summary>
/// Runtime host for Stageplay's Foster implementation.
/// </summary>
[PublicAPI]
public sealed class StageplayRuntime : App
{
    internal record StartupInfo(Func<StageplayRuntime, Game> GameFactory, GameInfo GameInfo, CommandLine CommandLine);
    internal record PlatformSystemImplementations(Func<StageplayRuntime, IPlatformAchievements>? AchievementsFactory);

    /// <summary>
    /// Creates a new runtime builder object.
    /// </summary>
    /// <param name="args">The program's command line arguments.</param>
    /// <param name="gameInfo">The game info for this game.</param>
    /// <typeparam name="TGame">The custom game logic class for this game.</typeparam>
    /// <returns></returns>
    public static StageplayRuntimeBuilder CreateWithGame<TGame>(ReadOnlySpan<string> args, GameInfo gameInfo)
        where TGame : Game, new()
    {
        return new StageplayRuntimeBuilder(new StartupInfo(rt => new TGame { Runtime = rt }, gameInfo, new CommandLine(args)));
    }
    
    /// <summary>
    /// The currently active runtime instance.
    /// </summary>
    public static StageplayRuntime? Current { get; private set; }
    
    /// <summary>
    /// The content/file loading API for the runtime.
    /// </summary>
    public ContentManager Content { get; }
    
    /// <summary>
    /// The custom game logic class for the runtime.
    /// </summary>
    public Game Game { get; }
    
    /// <summary>
    /// The game description for the runtime.
    /// </summary>
    public GameInfo GameInfo { get; }
    
    /// <summary>
    /// The parsed command line arguments for the runtime.
    /// </summary>
    public CommandLine CommandLineArguments { get; }
    
    /// <summary>
    /// The global Lua VM for the runtime.
    /// </summary>
    public LuaVM Lua { get; }
    
    /// <summary>
    /// The platform's achievement provider, if one exists.
    /// </summary>
    public IPlatformAchievements? Achievements { get; }

    /// <summary>
    /// Invoked when the runtime is starting up.
    /// </summary>
    public event Action? OnStartup;
    
    /// <summary>
    /// Invoked every frame before any other update work is done.
    /// </summary>
    public event Action? OnPreUpdate;
    
    /// <summary>
    /// Invoked after all other game systems have shut down.
    /// </summary>
    public event Action? OnShutdown;
    
    /// <summary>
    /// Creates a new app instance. Do not call this directly, it needs to be public for dependency injection to be able to create it.
    /// </summary>
    internal StageplayRuntime(StartupInfo info, PlatformSystemImplementations platformImpl, IEnumerable<Action<StageplayRuntime>> initCallbacks) : base(MakeAppConfigFromStartupInfo(info))
    {
        Current = this;
        
        Log.Info($"Stageplay {GitVersionInformation.SemVer}.{GitVersionInformation.ShortSha}");
        Log.Info($"Framework: {RuntimeInformation.FrameworkDescription}");
        Log.Info($"Platform: {RuntimeInformation.OSDescription} {RuntimeInformation.ProcessArchitecture}");

        CommandLineArguments = info.CommandLine;
        GameInfo = info.GameInfo;
        Game = info.GameFactory(this);
        Content = new ContentManager(this);
        Lua = new LuaVM(this);

        if (platformImpl.AchievementsFactory is not null)
            Achievements = platformImpl.AchievementsFactory(this);

        foreach (var cb in initCallbacks)
            cb(this);
    }

    /// <inheritdoc/>
    protected override void Startup()
    {
        Content.TitleStorageReady += ActualStartup;
        Content.LoadTitleStorage();
    }
    
    private void ActualStartup()
    {
        Log.Info("Runtime startup");
        OnStartup?.Invoke();
        
        Game.MountContent(Content);
        Lua.LoadMainModule();
        
        Game.PostStartup();
    }

    /// <inheritdoc/>
    protected override void Shutdown()
    {
        Log.Info("Runtime shutdown");
        
        Game.PreShutdown();
        Lua.Dispose();
        Content.Dispose();
        OnShutdown?.Invoke();

        if (Current == this)
            Current = null;
    }

    /// <inheritdoc/>
    protected override void Update()
    {
        // Don't do anything until title storage has loaded.
        if (!Content.IsTitleStorageReady)
            return;
        
        OnPreUpdate?.Invoke();
    }

    /// <inheritdoc/>
    protected override void Render()
    {
        // Don't do anything until title storage has loaded.
        if (!Content.IsTitleStorageReady)
        {
            Window.Clear(Color.Black);
            return;
        }
        
        Window.Clear(Color.CornflowerBlue);
    }
    
    private static AppConfig MakeAppConfigFromStartupInfo(StartupInfo startupInfo)
    {
        var sz = startupInfo.GameInfo.DesignSize;
        var fullscreen = true;

        if (startupInfo.CommandLine.TryGetValue("w", out var w) && int.TryParse(w, out var ww))
            sz.Width = ww;

        if (startupInfo.CommandLine.TryGetValue("h", out var h) && int.TryParse(h, out var hh))
            sz.Height = hh;

        if (startupInfo.CommandLine.Contains("window") || startupInfo.CommandLine.Contains("windowed"))
            fullscreen = false;

        var flags = AppFlags.NoHeaderLog;
        if (startupInfo.CommandLine.Contains("gpuDebug"))
            flags |= AppFlags.GraphicsDebugging;
        
        return new AppConfig(startupInfo.GameInfo.ApplicationName, startupInfo.GameInfo.ApplicationName, sz.Width, sz.Height, fullscreen, false,
            UpdateMode.UnlockedStep(), Flags: flags);
    }
}
