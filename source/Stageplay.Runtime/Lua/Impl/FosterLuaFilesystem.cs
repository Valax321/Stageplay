using Lua.IO;
using Microsoft.Extensions.DependencyInjection;
using Radish.Content;

namespace Radish.Lua.Impl;

internal sealed class FosterLuaFilesystem : ILuaFileSystem
{
    private readonly Lazy<ContentManager> _content;

    public FosterLuaFilesystem(StageplayRuntime app)
    {
        _content = app.Services.GetRequiredService<Lazy<ContentManager>>();
    }

    public bool IsReadable(string path)
    {
        return _content.Value.FileExists(path);
    }

    public ValueTask<ILuaStream> Open(string path, LuaFileOpenMode mode, CancellationToken cancellationToken)
    {
        if (mode is not LuaFileOpenMode.Read)
            throw new PlatformNotSupportedException("Only read-only lua streams are supported");

        var fs = _content.Value.OpenRead(path) 
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