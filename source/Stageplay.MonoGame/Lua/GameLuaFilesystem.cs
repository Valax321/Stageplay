using Lua.IO;
using Microsoft.Xna.Framework;

namespace Radish.MonoGame.Lua;

/// <summary>
/// This lua FS implementation is designed to be as sandboxed as possible.
/// </summary>
internal sealed class GameLuaFilesystem : ILuaFileSystem
{
    // TODO: verify the semantics of this -- it is supposed to return false if the file doesn't exist?
    public bool IsReadable(string path)
    {
        try
        {
            using var stream = TitleContainer.OpenStream(path);
            return true;
        }
        catch (FileNotFoundException)
        {
            // yuck! using exceptions to handle common runtime failure conditions
            return false;
        }
    }

    public ValueTask<ILuaStream> Open(string path, LuaFileOpenMode mode, CancellationToken cancellationToken)
    {
        if (mode is not LuaFileOpenMode.Read)
            throw new PlatformNotSupportedException("Writable lua streams not supported on MonoGame");

        var stream = TitleContainer.OpenStream(path);
        return new ValueTask<ILuaStream>(new LuaStream(mode, stream));
    }

    public ValueTask Rename(string oldName, string newName, CancellationToken cancellationToken)
    {
        throw new PlatformNotSupportedException("File modification not supported on MonoGame");
    }

    public ValueTask Remove(string path, CancellationToken cancellationToken)
    {
        throw new PlatformNotSupportedException("File modification not supported on MonoGame");
    }

    public string GetTempFileName()
    {
        throw new NotSupportedException("Temp files not supported on MonoGame");
    }

    public ValueTask<ILuaStream> OpenTempFileStream(CancellationToken cancellationToken)
    {
        throw new NotSupportedException("Temp files not supported on MonoGame");
    }
}