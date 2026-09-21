using Lua.IO;

namespace Radish.Lua.Impl;

internal sealed class FosterLuaStandardIO : ILuaStandardIO
{
    public ILuaStream Input => throw new NotImplementedException();
    public ILuaStream Output => throw new NotImplementedException();
    public ILuaStream Error => throw new NotImplementedException();
}