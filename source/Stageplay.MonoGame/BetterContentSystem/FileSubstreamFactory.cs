using Microsoft.Xna.Framework;
using Radish.IO;

namespace Radish.MonoGame.BetterContentSystem;

internal sealed class TitleContainerSubstreamFactory(string path) : ISubStreamFactory
{
    public Stream OpenWithOffset(long offset, long length)
    {
        var fs = TitleContainer.OpenStream(path);
        return new SubStream(fs, offset, length);
    }
}