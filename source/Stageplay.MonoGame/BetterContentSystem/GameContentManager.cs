using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Radish.MonoGame.Lua;

namespace Radish.MonoGame.BetterContentSystem;

/// <summary>
/// Content manager that bypasses the terrible XNA content pipeline.
/// It is much simpler and pretty much does the same things as the content pipeline.
/// The public interface is exactly the same as the original content manager.
/// </summary>
/// <param name="serviceProvider"></param>
/// <param name="rootDirectory"></param>
internal sealed class GameContentManager(IServiceProvider serviceProvider, string rootDirectory)
    : ContentManager(serviceProvider, rootDirectory)
{
    private readonly Dictionary<Type, ContentLoader> _loaders = new()
    {
        { typeof(LuaBytecode), new LuaBytecodeContentLoader() }
    };

    public override T Load<T>(string assetName)
    {
        if (LoadedAssets.TryGetValue(assetName, out var a))
            return (T)a;
        
        if (!_loaders.TryGetValue(typeof(T), out var loader))
            throw new ContentLoadException($"No ContentLoader found for {typeof(T).FullName}");

        try
        {
            using var stream = OpenStream($"{assetName}.{loader.FileExtension}");
            var newAsset = (T)loader.Load(stream);
            LoadedAssets[assetName] = newAsset;
            return newAsset;
        }
        catch (FileNotFoundException ex)
        {
            throw new ContentLoadException($"Could not open stream for asset: {ex.Message}");
        }
    }

    protected override Stream OpenStream(string assetName)
    {
        return TitleContainer.OpenStream(Path.Combine(RootDirectory, assetName));
    }
}