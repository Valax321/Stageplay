using Foster.Framework;
using JetBrains.Annotations;

namespace Radish;

/// <summary>
/// Handles the loading of assets in the runtime.
/// </summary>
[PublicAPI]
public interface IContentManager
{
    /// <summary>
    /// If true, then title storage is ready to go.
    /// </summary>
    bool IsTitleStorageReady { get; }
    
    GraphicsDevice? GraphicsDevice { get; }

    /// <summary>
    /// Tries to mount a fsarc file from the game's title storage.
    /// </summary>
    /// <param name="archiveName">The name of the archive to mount, without the extension.</param>
    /// <returns>True if the archive could be mounted, otherwise false.</returns>
    bool AddFsArcFile(string archiveName);
    
    /// <summary>
    /// Checks if the given file is present in the game's filesystem.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns>True if found, otherwise false.</returns>
    bool FileExists(string path);
    
    /// <summary>
    /// Opens a stream for the given file.
    /// </summary>
    /// <param name="path">The path to a file to open.</param>
    /// <returns>The stream, or null if it could not be opened.</returns>
    Stream? OpenRead(string path);
    
    /// <summary>
    /// Enumerates the files and directories present in the filesystem at the given path.
    /// </summary>
    /// <param name="path">The path to search. If null, the root directory is searched.</param>
    /// <param name="searchPattern">A glob pattern to use when filtering the results.</param>
    /// <param name="searchOption">Should the search be performed recursively?</param>
    /// <returns>The files found in the filesystem.</returns>
    IEnumerable<string> EnumerateDirectory(string? path = null, string? searchPattern = null,
        SearchOption searchOption = SearchOption.TopDirectoryOnly);

    /// <summary>
    /// Check if an asset exists and if it can be loaded as the given type.
    /// </summary>
    /// <param name="assetName">The asset name to load, without its extension.</param>
    /// <typeparam name="T">The type of asset to load.</typeparam>
    /// <returns>True if it can be loaded, otherwise false.</returns>
    bool Exists<T>(string assetName) where T : class;
    
    /// <summary>
    /// Performs a blocking load of the asset.
    /// If loading fails for any reason, an exception is thrown.
    /// </summary>
    /// <seealso cref="LoadAsync"/>
    /// <param name="assetName">The asset name to load, without its extension.</param>
    /// <typeparam name="T">The type of asset to load.</typeparam>
    /// <returns>The loaded asset.</returns>
    T Load<T>(string assetName) where T : class;
    
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
    ValueTask<T> LoadAsync<T>(string assetName, CancellationToken token = new()) where T : class;
}