using JetBrains.Annotations;

namespace Radish.Content;

/// <summary>
/// Base class for typed game asset loaders.
/// </summary>
[PublicAPI]
public abstract class ContentLoader
{
    /// <summary>
    /// The extension for the cooked asset.
    /// </summary>
    /// <param name="content">The <see cref="ContentManager"/> requesting the load.</param>
    /// <param name="originalPath">The asset path without the loader-specific extension.</param>
    public abstract string GetFileExtension(ContentManager content, string originalPath);

    /// <summary>
    /// Loads the asset object from the given stream.
    /// </summary>
    /// <param name="content">The <see cref="ContentManager"/> requesting the load.</param>
    /// <param name="source">The stream to load from.</param>
    /// <param name="token"></param>
    /// <returns>The loaded asset.</returns>
    public abstract ValueTask<object> Load(ContentManager content, Stream source, CancellationToken token = new());
}

/// <summary>
/// A typed interface over <see cref="ContentLoader"/>.
/// </summary>
/// <typeparam name="T">The type of asset to load.</typeparam>
public abstract class ContentLoader<T> : ContentLoader
    where T : class
{
    /// <summary>
    /// Loads the asset object from the given stream.
    /// </summary>
    /// <param name="content">The <see cref="ContentManager"/> requesting the load.</param>
    /// <param name="source">The stream to load from.</param>
    /// <param name="token"></param>
    /// <returns>The loaded asset.</returns>
    protected abstract ValueTask<T> LoadTyped(ContentManager content, Stream source, CancellationToken token);

    /// <inheritdoc/>
    public override async ValueTask<object> Load(ContentManager content, Stream source, CancellationToken token = new()) 
        => await LoadTyped(content, source, token);
}