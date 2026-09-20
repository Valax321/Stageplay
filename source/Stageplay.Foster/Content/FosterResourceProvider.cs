using Radish.Resources;

namespace Radish.Foster.Content;

public class FosterResourceProvider<TInterface, TImpl>(ContentManager content) 
    : IResourceProvider<TInterface>
    where TInterface : class
    where TImpl : class, TInterface
{
    private record Ref(string Path, TImpl Asset);
    
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
    
    private LinkedListNode<Ref>? FindInCacheByAsset(TInterface asset)
    {
        for (var n = _cache.First; n != null; n = n.Next)
        {
            if (n.Value.Asset == asset)
                return n;
        }

        return null;
    }
    
    public TInterface? LoadSync(string path)
    {
        var refNode = FindInCacheByPath(path);
        if (refNode is not null)
            return refNode.Value.Asset;

        if (!content.Exists<TImpl>(path))
            return null;

        var asset = content.Load<TImpl>(path);
        
        _cache.AddLast(new Ref(path, asset));
        return asset;
    }

    public IResourceLoadOperation<TInterface>? LoadAsync(string path)
    {
        var refNode = FindInCacheByPath(path);
        if (refNode is not null)
            return new FosterEmptyLoadOperation<TInterface>(refNode.Value.Asset);
        
        if (!content.Exists<TImpl>(path))
            return null;

        var cts = new CancellationTokenSource();
        var task = content.LoadAsync<TImpl>(path, cts.Token);
        return new FosterTaskLoadOperation<TInterface, TImpl>(task.AsTask(), cts);
    }

    public void Unload(TInterface asset)
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