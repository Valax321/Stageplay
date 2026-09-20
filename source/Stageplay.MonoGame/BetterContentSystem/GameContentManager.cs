using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Radish.IO;
using Radish.MonoGame.Graphics;
using Radish.MonoGame.Lua;

namespace Radish.MonoGame.BetterContentSystem;

/// <summary>
/// Content manager that bypasses the terrible XNA content pipeline.
/// It is much simpler and pretty much does the same things as the content pipeline.
/// The public interface is exactly the same as the original content manager.
/// </summary>
/// <param name="serviceProvider"></param>
/// <param name="rootDirectory"></param>
public sealed class GameContentManager(IServiceProvider serviceProvider, string rootDirectory)
    : ContentManager(serviceProvider, rootDirectory)
{
    private readonly Dictionary<Type, ContentLoader> _loaders = new()
    {
        { typeof(LuaBytecode), new LuaBytecodeContentLoader() },
        { typeof(Texture2D), new Texture2DContentLoader() }
    };

    private readonly LinkedList<FsArcFile> _paks = [];

    /// <summary>
    /// Tries the load the given FSARC file.
    /// </summary>
    /// <param name="pakName">The name of the archive file. Relative to root directory, don't include extension.</param>
    /// <returns>True if loaded successfully, otherwise false.</returns>
    [PublicAPI]
    public bool AddFsArcFile(string pakName)
    {
        var pakPath = Path.Combine(RootDirectory, $"{pakName}.{FsArcFile.FileExtension}");
        try
        {
            _paks.AddLast(FsArcFile.OpenRead(TitleContainer.OpenStream(pakPath),
                new TitleContainerSubstreamFactory(pakPath), pakPath));

            return true;
        }
        catch (FileNotFoundException)
        {
            Log.Warning($"Failed to mount fsarc file {pakPath}");
            return false;
        }
    }

    /// <inheritdoc/>
    public override T Load<T>(string assetName)
    {
        if (LoadedAssets.TryGetValue(assetName, out var a))
            return (T)a;

        if (!_loaders.TryGetValue(typeof(T), out var loader))
            throw new ContentLoadException($"No ContentLoader found for {typeof(T).FullName}");

        // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
        loader.Content ??= this;

        try
        {
            using var stream = OpenStream($"{assetName}.{loader.GetFileExtension(assetName)}");
            var newAsset = (T)loader.Load(stream);
            LoadedAssets[assetName] = newAsset;
            return newAsset;
        }
        catch (FileNotFoundException ex)
        {
            throw new ContentLoadException($"Could not open stream for asset: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    protected override Stream OpenStream(string assetName)
    {
        foreach (var pak in _paks)
        {
            if (pak.FileExists(assetName))
                return pak.OpenRead(assetName);
        }
        
        return TitleContainer.OpenStream(Path.Combine(RootDirectory, assetName));
    }
}