using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using MonoGame.Framework.Content.Pipeline.Builder;
using Radish.Lua;

namespace Radish;

public class StageplayBuilder : ContentBuilder
{
    public override IContentCollection GetContentCollection()
    {
        var content = new ContentCollection();
        content.SetContentRoot(string.Empty);
        
        content.Include<WildcardRule>("**/*.png", new TextureImporter(), new TextureProcessor
        {
            ColorKeyEnabled = false,
            MakeSquare = false,
            ResizeToPowerOfTwo = false,
            TextureFormat = TextureProcessorOutputFormat.Color,
            PremultiplyAlpha = true
        });

        content.Include<WildcardRule>("scripts/**/*.lua", 
            new LuaBytecodeImporter(), 
            new LuaBytecodeProcessor()
        );

        return content;
    }
}