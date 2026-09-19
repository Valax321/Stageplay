using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;

namespace Radish.MonoGame;

[PublicAPI]
public class StageplayGame : Game, IStandaloneSystemHost
{
    public GraphicsDeviceManager GraphicsDeviceManager { get; }
    public IAudioProvider AudioProvider => _audioProvider;
    public ITimeProvider TimeProvider => _timeProvider;

    public event MainLoopUpdateDelegate? OnUpdate;
    public event HostShutdownDelegate? OnShutdown;

    private GameAudioProvider _audioProvider;
    private GameTimeProvider _timeProvider;
    private GameInfo _gameInfo;

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

    protected override void LoadContent()
    {
        
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        _timeProvider.TotalElapsed = gameTime.TotalGameTime;
        _timeProvider.DeltaTime = gameTime.ElapsedGameTime;

        OnUpdate?.Invoke();
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        base.Draw(gameTime);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            OnShutdown?.Invoke();
        }
    }

    #region Host interface

    void IStageplaySystemHost.Initialize()
    {
    }

    public void RunFrame()
    {
    }

    #endregion
}