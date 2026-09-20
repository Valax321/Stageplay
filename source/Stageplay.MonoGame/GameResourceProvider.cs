using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework.Content;
using Radish.Resources;

namespace Radish.MonoGame;

internal sealed class GameResourceProvider<TInterface, TConcrete>(StageplayGame game) 
    : IResourceProvider<TInterface> 
    where TInterface : notnull
    where TConcrete : TInterface
{
    // Because MonoGame doesn't support async loading, this implementation is trivial.
    private sealed class LoadOp(TInterface asset) : IResourceLoadOperation<TInterface>
    {
        public bool IsCompleted => true;
        
        public TInterface Result => asset;
        
        public void Cancel()
        {
            // no-op
        }
    }
    
    private readonly Dictionary<string, TConcrete> _cache = new(StringComparer.InvariantCulture);
    private readonly Dictionary<TConcrete, string> _reverseLookup = [];
    
    public TInterface? LoadSync(string path)
    {
        if (TryGetFromCache(path, out var asset))
            return asset;

        var a = TryLoad(path);
        if (a is null)
            return default;
        
        _cache.Add(path, a);
        _reverseLookup.Add(a, path);
        return a;
    }

    public IResourceLoadOperation<TInterface>? LoadAsync(string path)
    {
        if (TryGetFromCache(path, out var asset))
            return new LoadOp(asset);
        
        var a = TryLoad(path);
        if (a is null)
            return null;
        
        _cache.Add(path, a);
        _reverseLookup.Add(a, path);
        return new LoadOp(a);
    }

    private bool TryGetFromCache(string path, [NotNullWhen(true)] out TConcrete? asset)
    {
        return _cache.TryGetValue(path, out asset);
    }

    private TConcrete? TryLoad(string path)
    {
        try
        {
            return game.Content.Load<TConcrete>(path);
        }
        catch (ContentLoadException)
        {
            return default;
        }
    }

    public void Unload(TInterface asset)
    {
        // This may
        if (!_reverseLookup.TryGetValue((TConcrete)asset, out var myPath))
        {
            Debug.Assert(false, "Reverse lookup of asset path failed, this may be a bug");
        }
        
        _cache.Remove(myPath);
        _reverseLookup.Remove((TConcrete)asset);
        
        game.Content.UnloadAsset(myPath);
    }
}