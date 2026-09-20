using Radish.Lua;
using Radish.MonoGame.Lua;

namespace Radish.MonoGame;

internal sealed class GameResources(StageplayGame game) : IStageplayResources
{
    public IResourceProvider<ILuaModule> LuaModules { get; } 
        = new GameResourceProvider<ILuaModule, LuaBytecode>(game);
}