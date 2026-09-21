using Radish.Content;

namespace Radish;

public sealed class ExampleGame : Game
{
    public override void MountContent(ContentManager content)
    {
        content.AddFsArcFile("data01");
    }
}