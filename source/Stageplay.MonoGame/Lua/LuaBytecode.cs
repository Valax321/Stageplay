using Lua;
using Radish.Lua;

namespace Radish.MonoGame.Lua;

/// <summary>
/// Lua bytecode serialized into an XNB container.
/// </summary>
/// <param name="bytecode"></param>
public sealed class LuaBytecode(byte[] bytecode) : ILuaModule
{
    /// <summary>
    /// The actual bytecode stream.
    /// </summary>
    public byte[] Bytecode => bytecode;
    
    /// <inheritdoc/>
    public LuaModule CreateModule(string name)
    {
        return new LuaModule(name, bytecode);
    }
}