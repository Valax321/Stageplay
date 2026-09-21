using Lua.IO;

namespace Radish.Lua.Impl;

internal sealed class FosterLuaFilesystem(StageplayRuntime app) : ILuaFileSystem
{
    public bool IsReadable(string path)
    {
        return app.Content.FileExists(path);
    }

    public ValueTask<ILuaStream> Open(string path, LuaFileOpenMode mode, CancellationToken cancellationToken)
    {
        if (mode is not LuaFileOpenMode.Read)
            throw new PlatformNotSupportedException("Only read-only lua streams are supported");

        var fs = app.Content.OpenRead(path) 
                 ?? throw new FileNotFoundException("Could not open stream for path", path);

        return new ValueTask<ILuaStream>(new LuaStream(mode, fs));
    }

    public ValueTask Rename(string oldName, string newName, CancellationToken cancellationToken)
    {
        throw new PlatformNotSupportedException("Game filesystem is read-only");
    }

    public ValueTask Remove(string path, CancellationToken cancellationToken)
    {
        throw new PlatformNotSupportedException("Game filesystem is read-only");
    }

    public string GetTempFileName()
    {
        throw new PlatformNotSupportedException("Game filesystem is read-only");
    }

    public ValueTask<ILuaStream> OpenTempFileStream(CancellationToken cancellationToken)
    {
        throw new PlatformNotSupportedException("Game filesystem is read-only");
    }
}