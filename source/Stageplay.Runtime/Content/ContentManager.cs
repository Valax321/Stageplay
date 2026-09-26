#define USE_DIRECTORY_STORAGE
using Foster.Framework;
using JetBrains.Annotations;
using Radish.IO;
using SDL3;

namespace Radish.Content;

/// <summary>
/// Manages the loading of game content using file-based APIs, and a typed content loader API.
/// </summary>
[PublicAPI]
public sealed class ContentManager : IDisposable
{
    /// <summary>
    /// If true, title storage is ready and will be used for asset loading.
    /// </summary>
    public bool IsTitleStorageReady => _titleStorage is not null;

    /// <summary>
    /// The graphics device associated with this content manager.
    /// </summary>
    public GraphicsDevice GraphicsDevice => _app.GraphicsDevice;

    internal event Action? TitleStorageReady;

    private StorageContainer? _titleStorage;
    private readonly LinkedList<FsArcStorage> _archives = [];
    private readonly string _titleStoragePath;
    private readonly StageplayRuntime _app;

    internal ContentManager(StageplayRuntime app)
    {
        _app = app;

        var titleStoragePath = Path.Combine(SDL.SDL_GetBasePath(), app.GameInfo.ContentDirectory);

        if (app.CommandLineArguments.TryGetValue("content", out var customContentPath))
        {
            titleStoragePath = Path.GetFullPath(customContentPath);
        }

        _titleStoragePath = titleStoragePath;
    }

    internal void LoadTitleStorage()
    {
        Log.Info($"Title storage path: {_titleStoragePath}");

        // Look, SDL_Storage should in theory be a great idea, but the fact
        // that I'm forced to load the entire file into memory makes it unsuitable
        // for any application that actually needs to *stream* data (the entire point for SDL even
        // having an IOStream interface in the first place!)
        // FsArc uses the normal .NET io stuff anyway, so until I literally hit a brick wall of a platform
        // that won't let me use any other file API I'm gonna do it.
#if !USE_DIRECTORY_STORAGE
        _app.FileSystem.OpenTitleStorage(_titleStoragePath, s =>
        {
            _titleStorage = s;
            TitleStorageReady?.Invoke();
        });
#else
        _titleStorage = new DirectoryStorage(_titleStoragePath);
        TitleStorageReady?.Invoke();
#endif
    }

    /// <summary>
    /// Tries to mount a fsarc file from the game's title storage.
    /// </summary>
    /// <param name="archiveName">The name of the archive to mount, without the extension.</param>
    /// <returns>True if the archive could be mounted, otherwise false.</returns>
    public bool AddFsArcFile(string archiveName)
    {
        var fullArchivePath = Path.Combine(_titleStoragePath, $"{archiveName}.{FsArcFile.FileExtension}");
        var arcStorage = FsArcStorage.TryOpen(new FileInfo(fullArchivePath));
        if (arcStorage is null)
        {
            Log.Warning(
                $"Failed to mount fsarc file {fullArchivePath}");
            return false;
        }

        _archives.AddLast(arcStorage);
        return true;
    }

    /// <summary>
    /// Checks if the given file is present in the game's filesystem.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns>True if found, otherwise false.</returns>
    public bool FileExists(string path)
    {
        foreach (var arc in _archives)
        {
            var exists = arc.FileExists(path);
            if (exists)
                return true;
        }

        return _titleStorage?.FileExists(path) ?? false;
    }

    /// <summary>
    /// Opens a stream for the given file.
    /// </summary>
    /// <param name="path">The path to a file to open.</param>
    /// <returns>The stream, or null if it could not be opened.</returns>
    public Stream? OpenRead(string path)
    {
        foreach (var arc in _archives)
        {
            var exists = arc.FileExists(path);
            if (!exists)
                continue;

            return arc.OpenRead(path);
        }

        if (!(_titleStorage?.FileExists(path) ?? false))
            return null;

        return _titleStorage.OpenRead(path);
    }
    
    public Stream OpenReadOrThrow(string path) 
        => OpenRead(path) ?? throw new FileNotFoundException("Could not open file from content manager", path);
    
    /// <summary>
    /// Enumerates the files and directories present in the filesystem at the given path.
    /// </summary>
    /// <remarks>
    /// Due to this method's implementation, a file will be returned more than once if it exists in more than one search path.
    /// It is the caller's responsibility to discard duplicate paths if necessary.
    /// </remarks>
    /// <param name="path">The path to search. If null, the root directory is searched.</param>
    /// <param name="searchPattern">A glob pattern to use when filtering the results.</param>
    /// <param name="searchOption">Should the search be performed recursively?</param>
    /// <returns>The files found in the filesystem.</returns>
    public IEnumerable<string> EnumerateDirectory(string? path = null, string? searchPattern = null,
        SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        foreach (var arc in _archives)
        {
            foreach (var f in arc.EnumerateDirectory(path, searchPattern, searchOption))
                yield return f;
        }

        if (_titleStorage is null)
            yield break;

        foreach (var f in _titleStorage.EnumerateDirectory(path, searchPattern, searchOption))
            yield return f;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _titleStorage?.Dispose();
        foreach (var arc in _archives)
            arc.Dispose();
    }
}