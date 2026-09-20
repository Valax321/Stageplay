using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Radish.MonoGame.Lua;

namespace Radish.Lua;

[ContentTypeWriter]
public class LuaBytecodeWriter : ContentTypeWriter<LuaBytecode>
{
    public override string GetRuntimeReader(TargetPlatform targetPlatform) =>
        typeof(LuaBytecodeReader).AssemblyQualifiedName!;

    protected override void Write(ContentWriter output, LuaBytecode value)
    {
        output.Write(value.Bytecode.Length);
        output.Write(value.Bytecode);
    }
}