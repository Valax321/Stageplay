using Radish.MonoGame;

namespace Radish;

public sealed class ExampleGame(IServiceProvider services) : StageplayGame(services)
{
    protected override void LoadContent()
    {
        Content.AddFsArcFile("data01_pc");
        
        base.LoadContent();
    }
}