using Lua;
using MemoryPack;
using Radish.Foster.Content;
using Radish.Lua;
using Radish.Serialization;

namespace Radish.Foster.Lua;

[MemoryPackable]
public sealed partial class LuaBytecodeModule : ILuaModule, IBinarySerializable
{
    public static FourCC HeaderMagic { get; } = new("LUAC");

    public required byte[] Bytecode { get; init; }
    
    public LuaModule CreateModule(string name)
    {
        return new LuaModule(name, Bytecode);
    }
    
    #region Content Loader
    
    internal sealed class Loader : ContentLoader<LuaBytecodeModule>
    {
        public override string GetFileExtension(ContentManager content, string originalPath) => ".luac";

        protected override async ValueTask<LuaBytecodeModule> LoadTyped(ContentManager content, Stream source, CancellationToken token)
        {
            return await BinaryObject.FromStreamAsync<LuaBytecodeModule>(source);
        }
    }
    
    #endregion
}