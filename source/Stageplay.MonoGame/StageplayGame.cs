using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;

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
    /// Invoked by <see cref="Game.Update"/>.
    /// </summary>
    public event MainLoopUpdateDelegate? OnUpdate;
    
    /// <summary>
    /// Invoked by <see cref="Game.EndRun"/>.
    /// </summary>
    public event HostShutdownDelegate? OnShutdown;

    private GameAudioProvider _audioProvider;
    private GameTimeProvider _timeProvider;
    private GameInfo _gameInfo;

    /// <inheritdoc/>
    public StageplayGame(IServiceProvider services)
    {
        GraphicsDeviceManager = new GraphicsDeviceManager(this);

        _gameInfo = services.GetRequiredService<GameInfo>();
        _timeProvider = new GameTimeProvider();

        _audioProvider = new GameAudioProvider(this);
        Components.Add(_audioProvider);

        SetupStuffAfterCtor();
    }

    private void SetupStuffAfterCtor()
    {
        // Use the standard monogame/xna content directory.
        Content.RootDirectory = "Content";

        // Set the window title
        Window.Title = _gameInfo.ApplicationName;
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
        GraphicsDevice.Clear(Color.CornflowerBlue);

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