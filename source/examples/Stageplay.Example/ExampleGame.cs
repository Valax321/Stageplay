namespace Radish;

public sealed class ExampleGame : Game
{
    public override void MountContent(IContentManager content)
    {
        content.AddFsArcFile("data01");
    }
}