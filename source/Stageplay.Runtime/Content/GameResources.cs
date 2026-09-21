using JetBrains.Annotations;
using Radish.Resources;

namespace Radish.Content;

[PublicAPI]
public class GameResources(StageplayRuntime app)
{
    public ResourceProvider<LuaBytecodeModule> LuaModules { get; } = new(app.Content);
}