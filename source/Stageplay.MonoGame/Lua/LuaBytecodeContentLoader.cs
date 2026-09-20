using Radish.MonoGame.BetterContentSystem;
using Radish.Serialization;

namespace Radish.MonoGame.Lua;

internal sealed class LuaBytecodeContentLoader : ContentLoader<LuaBytecode>
{
    protected override LuaBytecode LoadTyped(Stream source)
    {
        return BinaryObject.FromStream<LuaBytecode>(source);
    }

    public override string FileExtension => "luac";
}