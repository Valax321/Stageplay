using Radish.MonoGame;

namespace Radish;

public sealed class ExampleGame(IServiceProvider services) : StageplayGame(services)
{
    protected override void LoadContent()
    {
        Content.AddArchiveFile("data000");
        
        base.LoadContent();
    }
}