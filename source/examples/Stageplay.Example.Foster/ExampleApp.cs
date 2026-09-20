using Radish.Foster;

namespace Radish;

public sealed class ExampleApp(IServiceProvider services) : StageplayApp(services)
{
    protected override void MountContent()
    {
        Content.AddFsArcFile("data01");
    }
}