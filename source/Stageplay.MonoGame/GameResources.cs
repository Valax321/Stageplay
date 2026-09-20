using Radish.Lua;
using Radish.MonoGame.Lua;

namespace Radish.MonoGame;

internal sealed class GameResources(StageplayGame game) : IResourcesProvider
{
    public IResourceProvider<ILuaModule> LuaModules { get; } 
        = new GameResourceProvider<ILuaModule, LuaBytecode>(game);
}