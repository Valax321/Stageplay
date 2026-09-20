using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radish.Lua;
using Radish.MonoGame.BetterContentSystem;

namespace Radish.MonoGame;

/// <summary>
/// MonoGame <see cref="Game"/> subclass that implements the main loop host for Stageplay.
/// </summary>
[PublicAPI]
public class StageplayGame : Game, IStandaloneSystemHost
{
    /// <summary>
    /// The game's graphics device manager.
    /// </summary>
    public GraphicsDeviceManager GraphicsDeviceManager { get; }
    
    /// <summary>
    /// Implements the audio provider for MonoGame.
    /// </summary>
    public IAudioProvider AudioProvider => _audioProvider;
    
    /// <summary>
    /// Implements the time provider for MonoGame.
    /// </summary>
    public ITimeProvider TimeProvider => _timeProvider;

    /// <summary>
    /// Implements the game resource provider for MonoGame.
    /// </summary>
    public IStageplayResources Resources => _gameResources;

    /// <summary>
    /// Invoked by <see cref="Game.Update"/>.
    /// </summary>
    public event HostUpdateDelegate? OnUpdate;
    
    /// <summary>
    /// Invoked by <see cref="Game.EndRun"/>.
    /// </summary>
    public event HostShutdownDelegate? OnShutdown;
    
    /// <summary>
    /// Invoked by <see cref="Game.LoadContent"/>
    /// </summary>
    public event HostStartupDelegate? OnStartup;

    private GameAudioProvider _audioProvider;
    private GameTimeProvider _timeProvider;
    private GameResources _gameResources;
    private GameInfo _gameInfo;

    /// <inheritdoc/>
    public StageplayGame(IServiceProvider services)
    {
        // I need you to understand how bad the default XNA content system is
        Content = new GameContentManager(Services, "Content");
        
        _gameInfo = services.GetRequiredService<GameInfo>();
        _timeProvider = new GameTimeProvider();

        _audioProvider = new GameAudioProvider(this);
        Components.Add(_audioProvider);

        _gameResources = new GameResources(this);

        var cmdLine = services.GetRequiredService<ICommandLineArguments>();

        var sz = _gameInfo.DesignSize;
        var fullscreen = true;

        if (cmdLine.TryGetValue("w", out var w) && int.TryParse(w, out var ww))
            sz.Width = ww;

        if (cmdLine.TryGetValue("h", out var h) && int.TryParse(h, out var hh))
            sz.Height = hh;

        if (cmdLine.ContainsKey("window") || cmdLine.ContainsKey("windowed"))
            fullscreen = false;
        
        GraphicsDeviceManager = new GraphicsDeviceManager(this)
        {
            GraphicsProfile = GraphicsProfile.Reach,
            SynchronizeWithVerticalRetrace = true,
            PreferredBackBufferWidth = sz.Width,
            PreferredBackBufferHeight = sz.Height,
            IsFullScreen = fullscreen
        };

        SetupStuffAfterCtor();
    }

    private void SetupStuffAfterCtor()
    {
        IsMouseVisible = true;
        
        // Set the window title
        Window.Title = _gameInfo.ApplicationName;
    }

    /// <inheritdoc/>
    protected override void LoadContent()
    {
        OnStartup?.Invoke();
    }

    /// <inheritdoc/>
    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        _timeProvider.TotalElapsed = gameTime.TotalGameTime;
        _timeProvider.DeltaTime = gameTime.ElapsedGameTime;
        
        OnUpdate?.Invoke();
    }

    /// <inheritdoc/>
    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        base.Draw(gameTime);
    }

    /// <inheritdoc/>
    protected override void EndRun()
    {
        OnShutdown?.Invoke();
    }

    #region Host interface

    /// <inheritdoc/>
    public void RunFrame()
    {
        throw new PlatformNotSupportedException(
            "The monogame backend does not support the per-frame main loop. Use RunWithMainLoop() instead");
    }

    #endregion
}