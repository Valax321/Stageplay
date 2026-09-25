using Foster.Framework;
using Radish.Graphics;
using Radish.IO;
using Radish.Resources;
using SDL3;

namespace Radish.Content;

/// <summary>
/// Manages the loading of game content using file-based APIs, and a typed content loader API.
/// </summary>
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

    private static readonly Dictionary<Type, ContentLoader> Loaders = [];

    static ContentManager()
    {
        RegisterLoader<Texture, TextureLoader>();
        RegisterLoader<LuaBytecodeModule, LuaBytecodeModule.Loader>();
        RegisterLoader<CompiledScenario, CompiledScenario.Loader>();
    }

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

        _app.FileSystem.OpenTitleStorage(_titleStoragePath, s =>
        {
            _titleStorage = s;
            TitleStorageReady?.Invoke();
        });
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

    /// <summary>
    /// Check if an asset exists and if it can be loaded as the given type.
    /// </summary>
    /// <param name="assetName">The asset name to load, without its extension.</param>
    /// <typeparam name="T">The type of asset to load.</typeparam>
    /// <returns>True if it can be loaded, otherwise false.</returns>
    public bool Exists<T>(string assetName) where T : class
    {
        if (!Loaders.TryGetValue(typeof(T), out var loader))
            return false;

        var path = Path.ChangeExtension(assetName, loader.GetFileExtension(this, assetName));
        return FileExists(path);
    }

    /// <summary>
    /// Performs a blocking load of the asset.
    /// If loading fails for any reason, an exception is thrown.
    /// </summary>
    /// <seealso cref="LoadAsync"/>
    /// <param name="assetName">The asset name to load, without its extension.</param>
    /// <typeparam name="T">The type of asset to load.</typeparam>
    /// <returns>The loaded asset.</returns>
    public T Load<T>(string assetName) where T : class
    {
        // Sync load just halts the current thread until the async task is finished.
        var task = LoadAsync<T>(assetName);
        if (task.IsCompleted)
            return task.Result;

        var tt = task.AsTask();
        tt.Wait();
        return tt.Result;
    }

    /// <summary>
    /// Performs a non-blocking load of the asset.
    /// If loading fails for any reason, an exception is thrown.
    /// </summary>
    /// <seealso cref="Load"/>
    /// <param name="assetName">The asset name to load, without its extension.</param>
    /// <param name="token">Cancellation token to allow cancelling the load task.</param>
    /// <typeparam name="T">The type of asset to load.</typeparam>
    /// <returns>The loaded asset.</returns>
    /// <exception cref="InvalidOperationException">Thrown if a loader is not registered for this asset type.</exception>
    /// <exception cref="FileNotFoundException">Thrown if a stream could not be opened for the asset's file.</exception>
    public async ValueTask<T> LoadAsync<T>(string assetName, CancellationToken token = new()) where T : class
    {
        if (!Loaders.TryGetValue(typeof(T), out var loader))
            throw new InvalidOperationException("Loading asset without a loader, check Exists() first");

        var path = Path.ChangeExtension(assetName, loader.GetFileExtension(this, assetName));
        await using var fs = OpenRead(path) ?? throw new FileNotFoundException(null, path);
        return (T)await loader.Load(this, fs, token);
    }

    /// <summary>
    /// Register a new loader class with the content manager.
    /// </summary>
    /// <typeparam name="TAsset">The type of asset being loaded.</typeparam>
    /// <typeparam name="TLoader">The type responsible for loading the asset.</typeparam>
    public static void RegisterLoader<TAsset, TLoader>()
        where TAsset : class
        where TLoader : ContentLoader<TAsset>, new()
    {
        Loaders.Add(typeof(TAsset), new TLoader());
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _titleStorage?.Dispose();
        foreach (var arc in _archives)
            arc.Dispose();
    }
}