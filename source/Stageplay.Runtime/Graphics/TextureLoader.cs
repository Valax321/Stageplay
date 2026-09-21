using Foster.Framework;
using Radish.Content;

namespace Radish.Graphics;

internal sealed class TextureLoader : ContentLoader<Texture>
{
    public override string GetFileExtension(IContentManager content, string originalPath) => ".qoi";

    protected override ValueTask<Texture> LoadTyped(IContentManager content, Stream source, CancellationToken token)
    {
        using var img = new Image(source);
        return new ValueTask<Texture>(new Texture(content.GraphicsDevice ?? throw new PlatformNotSupportedException(), img));
    }
}