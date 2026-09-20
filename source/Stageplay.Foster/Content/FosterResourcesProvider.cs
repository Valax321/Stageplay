using Radish.Foster.Lua;
using Radish.Lua;

namespace Radish.Foster.Content;

internal sealed class FosterResourcesProvider(StageplayApp app) : IResourcesProvider
{
    public IResourceProvider<ILuaModule> LuaModules { get; } =
        new FosterResourceProvider<ILuaModule, LuaBytecodeModule>(app.Content);
}