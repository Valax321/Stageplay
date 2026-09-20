using Microsoft.Xna.Framework.Content.Pipeline;
using Radish.MonoGame.Lua;

namespace Radish.Lua;

[ContentProcessor]
public class LuaBytecodeProcessor : ContentProcessor<LuaBytecode, LuaBytecode>
{
    public override LuaBytecode Process(LuaBytecode input, ContentProcessorContext context)
    {
        return input;
    }
}