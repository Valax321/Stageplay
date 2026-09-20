using Lua;
using MemoryPack;
using Radish.Lua;
using Radish.Serialization;

namespace Radish.MonoGame.Lua;

/// <summary>
/// Lua bytecode serialized into an XNB container.
/// </summary>
[MemoryPackable]
public sealed partial class LuaBytecode : ILuaModule, IBinarySerializable
{
    /// <inheritdoc/>
    public static FourCC HeaderMagic => new("LUAC");

    /// <summary>
    /// The actual bytecode stream.
    /// </summary>
    public required byte[] Bytecode { get; init; }
    
    /// <inheritdoc/>
    public LuaModule CreateModule(string name)
    {
        return new LuaModule(name, Bytecode);
    }
}