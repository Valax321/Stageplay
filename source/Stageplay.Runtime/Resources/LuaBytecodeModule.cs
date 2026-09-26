using System.IO.Compression;
using Lua;
using MemoryPack;
using Radish.Content;
using Radish.Lua;
using Radish.Serialization;

namespace Radish.Resources;

/// <summary>
/// Asset storing a blob of Lua bytecode.
/// </summary>
[MemoryPackable]
public sealed partial class LuaBytecodeModule : ILuaModule, IBinarySerializable
{
    /// <inheritdoc/>
    public static FourCC HeaderMagic { get; } = new("LUAC");

    /// <summary>
    /// The lua bytecode blob.
    /// </summary>
    [BrotliFormatter(CompressionLevel.SmallestSize)]
    public required byte[] Bytecode { get; init; }
    
    /// <summary>
    /// Creates a <see cref="LuaModule"/> containing the bytecode blob.
    /// </summary>
    /// <param name="name">The name of the module to create.</param>
    public LuaModule CreateModule(string name)
    {
        return new LuaModule(name, Bytecode);
    }

    public static async ValueTask<LuaBytecodeModule> LoadAsync(Stream stream)
    {
        return await BinaryObject.FromStreamAsync<LuaBytecodeModule>(stream);
    }
    
    public static LuaBytecodeModule Load(Stream stream)
    {
        return BinaryObject.FromStream<LuaBytecodeModule>(stream);
    }
}