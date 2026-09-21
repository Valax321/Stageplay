using Foster.Framework;
using Radish.IO;

namespace Radish.Content;

internal sealed class SimpleSubstreamFactory(FileInfo file) : ISubStreamFactory
{
    public Stream OpenWithOffset(long offset, long length)
    {
        return new SubStream(file.OpenRead(), offset, length);
    }
}

internal sealed class FsArcStorage : StorageContainer
{
    public override bool Writable => false;

    private readonly FsArcFile _arcFile;

    public static FsArcStorage? TryOpen(FileInfo file)
    {
        if (!file.Exists)
            return null;

        var arc = FsArcFile.OpenRead(file.OpenRead(), new SimpleSubstreamFactory(file), file.FullName);
        return new FsArcStorage(arc);
    }
    
    private FsArcStorage(FsArcFile arc)
    {
        _arcFile = arc;
    }
    
    public override bool FileExists(string path)
    {
        return _arcFile.FileExists(path);
    }

    public override bool DirectoryExists(string path)
    {
        return _arcFile.DirectoryExists(path);
    }

    public override Stream OpenRead(string path)
    {
        return _arcFile.OpenRead(path);
    }

    public override IEnumerable<string> EnumerateDirectory(string? path = null, string? searchPattern = null,
        SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        foreach (var (f, _) in _arcFile.EnumerateDirectory(path, searchOption))
        {
            //TODO: match against search pattern
            yield return f;
        }
    }

    public override void Dispose(bool disposing)
    {
    }
}