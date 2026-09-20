namespace Radish.IO;

/// <summary>
/// Creates streams of the same file with different offsets and lengths.
/// </summary>
public interface ISubStreamFactory
{
    /// <summary>
    /// Opens a stream with the given offset and length.
    /// </summary>
    /// <param name="offset">The offset to open at.</param>
    /// <param name="length">The length of the opened stream.</param>
    /// <returns>The new substream.</returns>
    Stream OpenWithOffset(long offset, long length);
}