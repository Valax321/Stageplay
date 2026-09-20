using Foster.Framework;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Radish.Foster.Graphics;
using Radish.Foster.Lua;
using Radish.IO;
using SDL3;

namespace Radish.Foster.Content;

[PublicAPI]
public sealed class ContentManager : IDisposable
{
    internal event Action? TitleStorageReady;

    public bool IsTitleStorageReady => _titleStorage is not null;
    public GraphicsDevice GraphicsDevice => _app.GraphicsDevice;
    
    private StorageContainer? _titleStorage;
    private readonly LinkedList<FsArcStorage> _archives = [];
    private readonly string _titleStoragePath;
    private StageplayApp _app;
    
    private static readonly Dictionary<Type, ContentLoader> Loaders = [];

    static ContentManager()
    {
        RegisterLoader<Texture, TextureLoader>();
        RegisterLoader<LuaBytecodeModule, LuaBytecodeModule.Loader>();
    }

    public ContentManager(StageplayApp app)
    {
        _app = app;
        
        var gameInfo = app.Services.GetRequiredService<GameInfo>();
        var cmdLine = app.Services.GetRequiredService<ICommandLineArguments>();
        
        var titleStoragePath = Path.Combine(SDL.SDL_GetBasePath(), gameInfo.ContentDirectory);
        
        if (cmdLine.TryGetValue("content", out var customContentPath))
        {
            titleStoragePath = Path.GetFullPath(customContentPath);
        }
        _titleStoragePath = titleStoragePath;
    }

    internal void LoadTitleStorage()
    {
        _app.FileSystem.OpenTitleStorage(_titleStoragePath, s =>
        {
            Log.Info("Title storage ready");
            _titleStorage = s;
            
            TitleStorageReady?.Invoke();
        });
    }

    public bool AddFsArcFile(string archiveName)
    {
        var fullArchivePath = Path.Combine(_titleStoragePath, $"{archiveName}.{FsArcFile.FileExtension}");
        var arcStorage = FsArcStorage.TryOpen(new FileInfo(fullArchivePath));
        if (arcStorage is null)
        {
            Log.Warning($"Failed to mount fsarc file {Path.GetRelativePath(AppContext.BaseDirectory, fullArchivePath)}");
            return false;
        }

        _archives.AddLast(arcStorage);
        return true;
    }

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

    public IEnumerable<string> EnumerateDirectory(string? path = null, string? searchPattern = null,
        SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        var items = new HashSet<string>(StringComparer.InvariantCulture);
        foreach (var arc in _archives)
        {
            foreach (var f in arc.EnumerateDirectory(path, searchPattern, searchOption))
                items.Add(f);
        }

        if (_titleStorage is null) 
            return items;
        
        foreach (var f in _titleStorage.EnumerateDirectory(path, searchPattern, searchOption))
            items.Add(f);

        return items;
    }

    public bool Exists<T>(string assetName) where T : class
    {
        if (!Loaders.TryGetValue(typeof(T), out var loader))
            return false;

        var path = Path.ChangeExtension(assetName, loader.GetFileExtension(this, assetName));
        return FileExists(path);
    }

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

    public async ValueTask<T> LoadAsync<T>(string assetName, CancellationToken token = new()) where T : class
    {
        if (!Loaders.TryGetValue(typeof(T), out var loader))
            throw new InvalidOperationException("Loading asset without a loader, check Exists() first");

        var path = Path.ChangeExtension(assetName, loader.GetFileExtension(this, assetName));
        await using var fs = OpenRead(path) ?? throw new FileNotFoundException(null, path);
        return (T)await loader.Load(this, fs, token);
    }
    
    public static void RegisterLoader<TAsset, TLoader>()
        where TAsset : class 
        where TLoader : ContentLoader<TAsset>, new()
    {
        Loaders.Add(typeof(TAsset), new TLoader());
    }
    
    public void Dispose()
    {
        _titleStorage?.Dispose();
        foreach (var arc in _archives)
            arc.Dispose();
    }
}