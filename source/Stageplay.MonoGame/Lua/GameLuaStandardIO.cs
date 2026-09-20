using Lua.IO;

namespace Radish.MonoGame.Lua;

internal sealed class GameLuaStandardIO : ILuaStandardIO
{
    public ILuaStream Input => throw new NotImplementedException();
    public ILuaStream Output => throw new NotImplementedException();
    public ILuaStream Error => throw new NotImplementedException();
}