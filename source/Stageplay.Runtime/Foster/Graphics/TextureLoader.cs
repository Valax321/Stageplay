using Foster.Framework;
using Radish.Foster.Content;

namespace Radish.Foster.Graphics;

internal sealed class TextureLoader : ContentLoader<Texture>
{
    public override string GetFileExtension(ContentManager content, string originalPath) => ".qoi";

    protected override ValueTask<Texture> LoadTyped(ContentManager content, Stream source, CancellationToken token)
    {
        using var img = new Image(source);
        return new ValueTask<Texture>(new Texture(content.GraphicsDevice, img));
    }
}