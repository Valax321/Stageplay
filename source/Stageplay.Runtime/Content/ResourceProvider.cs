using Radish.Resources;

namespace Radish.Content;

/// <summary>
/// Loads and caches assets of a single type.
/// </summary>
/// <param name="content">The content manager to load assets from/</param>
/// <typeparam name="TAsset">The type of the asset.</typeparam>
public sealed class ResourceProvider<TAsset>(ContentManager content)
    where TAsset : class
{
    private record Ref(string Path, TAsset Asset);
    
    private readonly LinkedList<Ref> _cache = [];

    private LinkedListNode<Ref>? FindInCacheByPath(string path)
    {
        for (var n = _cache.First; n != null; n = n.Next)
        {
            if (n.Value.Path.Equals(path, StringComparison.InvariantCulture))
                return n;
        }

        return null;
    }
    
    private LinkedListNode<Ref>? FindInCacheByAsset(TAsset asset)
    {
        for (var n = _cache.First; n != null; n = n.Next)
        {
            if (n.Value.Asset == asset)
                return n;
        }

        return null;
    }
    
    /// <summary>
    /// Loads an asset synchronously.
    /// </summary>
    /// <param name="path">The path to the asset, without its extension.</param>
    /// <returns>The loaded asset.</returns>
    public TAsset? Load(string path)
    {
        var refNode = FindInCacheByPath(path);
        if (refNode is not null)
            return refNode.Value.Asset;

        if (!content.Exists<TAsset>(path))
            return null;

        var asset = content.Load<TAsset>(path);
        
        _cache.AddLast(new Ref(path, asset));
        return asset;
    }

    /// <summary>
    /// Loads an asset asynchronously.
    /// </summary>
    /// <param name="path"></param>
    /// <returns>A handle wrapping the load task.</returns>
    public IResourceLoadOperation<TAsset>? LoadAsync(string path)
    {
        var refNode = FindInCacheByPath(path);
        if (refNode is not null)
            return new SyncResourceLoadOperation<TAsset>(refNode.Value.Asset);
        
        if (!content.Exists<TAsset>(path))
            return null;

        var cts = new CancellationTokenSource();
        var task = content.LoadAsync<TAsset>(path, cts.Token);
        return new TaskResourceLoadOperation<TAsset>(task.AsTask(), cts);
    }

    /// <summary>
    /// Unloads the given asset from the cache. If the asset implements <see cref="IDisposable"/>, then it will be disposed.
    /// </summary>
    /// <param name="asset">The asset to unload.</param>
    public void Unload(TAsset asset)
    {
        var refNode = FindInCacheByAsset(asset);
        if (refNode is not null)
        {
            if (refNode.Value.Asset is IDisposable d)
                d.Dispose();
            
            _cache.Remove(refNode);
        }
    }
}