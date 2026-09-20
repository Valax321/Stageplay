using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework.Graphics;
using Radish.MonoGame.BetterContentSystem;

namespace Radish.MonoGame.Graphics;

internal sealed class Texture2DContentLoader : ContentLoader<Texture2D>
{
    public override string GetFileExtension(string originalPath) => ".png";

    protected override Texture2D LoadTyped(Stream source)
    {
        var graphicsDevice = Content.ServiceProvider.GetRequiredService<GraphicsDevice>();
        return Texture2D.FromStream(graphicsDevice, source);
    }
}