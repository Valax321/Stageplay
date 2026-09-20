using Lua;
using Microsoft.Xna.Framework.Content.Pipeline;
using Radish.MonoGame.Lua;

namespace Radish.Lua;

[ContentImporter(".lua")]
public class LuaBytecodeImporter : ContentImporter<LuaBytecode>
{
    public override LuaBytecode Import(string filename, ContentImporterContext context)
    {
        var bc = File.ReadAllText(filename);
        
        using var state = LuaState.Create();
        var closure = state.Load(bc, Path.GetFileNameWithoutExtension(filename));
        return new LuaBytecode(closure.Proto.ToBytecode());
    }
}