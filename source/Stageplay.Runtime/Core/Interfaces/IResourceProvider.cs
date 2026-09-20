using JetBrains.Annotations;
using Radish.Resources;

namespace Radish;

/// <summary>
/// Interface for loading a single type of game asset.
/// </summary>
/// <typeparam name="T">The type of asset to load.</typeparam>
[PublicAPI]
public interface IResourceProvider<T> where T : class
{
    /// <summary>
    /// Loads the requested asset synchronously.
    /// </summary>
    /// <param name="path">The path to the asset.</param>
    /// <returns>The loaded asset, or null if it failed to be loaded.</returns>
    T? LoadSync(string path);
    
    /// <summary>
    /// Loads the requested asset asynchronously.
    /// </summary>
    /// <param name="path">The path to the asset.</param>
    /// <returns>The loaded asset, or null if it failed to be loaded.</returns>
    IResourceLoadOperation<T>? LoadAsync(string path);

    /// <summary>
    /// Requests to unload the given asset.
    /// </summary>
    /// <param name="asset">The asset to unload.</param>
    void Unload(T asset);
}