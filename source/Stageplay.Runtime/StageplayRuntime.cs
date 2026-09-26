using System.Diagnostics;
using System.Runtime.InteropServices;
using Foster.Framework;
using JetBrains.Annotations;
using Radish.Audio;
using Radish.Content;
using Radish.Debugger;
using Radish.Graphics;
using Radish.IO;
using Radish.Lua;
using Radish.Platform;
using Radish.Resources;
using Radish.Scenario;
using Radish.Scenario.Commands;
using SDL3;

namespace Radish;

/// <summary>
/// Runtime host for Stageplay's Foster implementation.
/// </summary>
[PublicAPI]
public sealed class StageplayRuntime : App
{
    internal record StartupInfo(Func<StageplayRuntime, Game> GameFactory, GameInfo GameInfo, CommandLine CommandLine, LocalSettingsStore? LocalSettingsStore);
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
        return new StageplayRuntimeBuilder(
            new StartupInfo(
                rt => new TGame { Runtime = rt }, 
                gameInfo, 
                new CommandLine(args),
                new LocalSettingsStore(gameInfo)
            )
        );
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
    /// Manages the audio system for the runtime.
    /// </summary>
    public AudioDevice Audio { get; }
    
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

    public ScenarioVM? ActiveScenario { get; private set; }
    
    public ScenarioSoundManager SoundManager { get; }

    internal DebugMenu DebugMenu { get; }

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

    private Renderer _renderer;
    private LocalSettingsStore? _settingsStore;

    /// <summary>
    /// Creates a new app instance. Do not call this directly, it needs to be public for dependency injection to be able to create it.
    /// </summary>
    internal StageplayRuntime(StartupInfo info, PlatformSystemImplementations platformImpl, IEnumerable<Action<StageplayRuntime>> initCallbacks) : base(MakeAppConfigFromStartupInfo(info))
    {
        Current = this;
        
        Log.Info($"Stageplay {GitVersionInformation.SemVer}.{GitVersionInformation.ShortSha}");
        Log.Info($"Framework: {RuntimeInformation.FrameworkDescription}");
        Log.Info($"Platform: {RuntimeInformation.OSDescription} {RuntimeInformation.ProcessArchitecture}");

        GraphicsDevice.VSync = true;
        
        CommandLineArguments = info.CommandLine;
        GameInfo = info.GameInfo;
        Game = info.GameFactory(this);
        Content = new ContentManager(this);
        Lua = new LuaVM(this);
        Audio = new AudioDevice();
        
        DebugMenu = new DebugMenu(this);
        _renderer = new Renderer(this);
        _settingsStore = info.LocalSettingsStore;

        SoundManager = new ScenarioSoundManager(Audio, Content);

        if (platformImpl.AchievementsFactory is not null)
            Achievements = platformImpl.AchievementsFactory(this);

        foreach (var cb in initCallbacks)
            cb(this);
    }

    /// <summary>
    /// Runs the application.
    /// </summary>
    [DebuggerDisableUserUnhandledExceptions]
    public new void Run()
    {
        try
        {
            base.Run();
        }
        catch (Exception ex)
        {
            if (System.Diagnostics.Debugger.IsAttached)
                System.Diagnostics.Debugger.BreakForUserUnhandledException(ex);
            
            // I've submitted a PR for adding a messagebox API to Foster.
            // Until that's done, just do it directly with the SDL api.
            // The benefit of the Foster implementation is being able to set the messagebox window
            // properly, so the popup will be forced on top of the game window.

            SDL.SDL_ShowSimpleMessageBox(SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR, "Fatal Error",
                $"The game encountered an unrecoverable error and will now close.\n{ex.Message}", 0);
            Environment.ExitCode = 1;
        }
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
        LoadInitScenario();
    }

    private void LoadInitScenario()
    {
        if (!LoadScenarioByName("init"))
        {
            Log.Error("Init scenario is missing. All Stageplay games must have at least a scenario named \"init.scenario\".");
        }
    }

    private bool LoadScenarioByName(string name)
    {
        var scenarioName = $"{name}.bscn";
        
        if (!Content.FileExists(scenarioName))
            return false;

        using var fs = Content.OpenReadOrThrow(scenarioName);
        var scenarioScript = CompiledScenario.Load(fs);
        ActiveScenario = new ScenarioVM(this, name, scenarioScript, RuntimeBuiltinCommands.Table);
        return true;
    }

    /// <inheritdoc/>
    protected override void Shutdown()
    {
        Log.Info("Runtime shutdown");
        
        Game.PreShutdown();
        
        Audio.Dispose();
        _renderer.Dispose();
        Lua.Dispose();
        Content.Dispose();
        OnShutdown?.Invoke();
        
        if (Current == this)
            Current = null;
    }

    private void SaveLocalResolutionSettings()
    {
        if (_settingsStore is not null)
        {
            _settingsStore.SetBool("Fullscreen", Window.Fullscreen);
            _settingsStore.SetInt("ResolutionX", Window.Width);
            _settingsStore.SetInt("ResolutionY", Window.Height);
            _settingsStore.Save();
        }
    }

    /// <inheritdoc/>
    protected override void Update()
    {
        // Don't do anything until title storage has loaded.
        if (!Content.IsTitleStorageReady)
            return;
        
        OnPreUpdate?.Invoke();
        ActiveScenario?.Update();
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
        
        _renderer.DrawFrame(Window);
    }
    
    private static AppConfig MakeAppConfigFromStartupInfo(StartupInfo startupInfo)
    {
        var sz = startupInfo.GameInfo.DesignSize;
        var fullscreen = true;

        if (startupInfo.LocalSettingsStore is not null)
        {
            if (!startupInfo.CommandLine.Contains("safemode"))
                startupInfo.LocalSettingsStore.Load();

            sz.X = startupInfo.LocalSettingsStore.GetInt("ResolutionX", sz.X);
            sz.Y = startupInfo.LocalSettingsStore.GetInt("ResolutionY", sz.Y);
            fullscreen = startupInfo.LocalSettingsStore.GetBool("Fullscreen", fullscreen);
        }

        if (startupInfo.CommandLine.TryGetValue("w", out var w) && int.TryParse(w, out var ww))
            sz.X = ww;

        if (startupInfo.CommandLine.TryGetValue("h", out var h) && int.TryParse(h, out var hh))
            sz.Y = hh;

        if (startupInfo.CommandLine.Contains("window") || startupInfo.CommandLine.Contains("windowed"))
            fullscreen = false;

        var flags = AppFlags.NoHeaderLog;
        if (startupInfo.CommandLine.Contains("gpuDebug"))
            flags |= AppFlags.GraphicsDebugging;
        
        return new AppConfig(startupInfo.GameInfo.ApplicationName, startupInfo.GameInfo.ApplicationName, sz.X, sz.Y, fullscreen, false,
            UpdateMode.UnlockedStep(), Flags: flags);
    }
}
