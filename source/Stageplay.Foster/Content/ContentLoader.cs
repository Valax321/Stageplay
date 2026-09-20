namespace Radish.Foster.Content;

public abstract class ContentLoader
{
    /// <summary>
    /// The extension for the cooked asset.
    /// </summary>
    public abstract string GetFileExtension(ContentManager content, string originalPath);

    /// <summary>
    /// Loads the asset object from the given stream.
    /// </summary>
    /// <param name="source">The stream to load from.</param>
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
    /// <param name="source">The stream to load from.</param>
    /// <returns>The loaded asset.</returns>
    protected abstract ValueTask<T> LoadTyped(ContentManager content, Stream source, CancellationToken token);

    /// <inheritdoc/>
    public override async ValueTask<object> Load(ContentManager content, Stream source, CancellationToken token = new()) 
        => await LoadTyped(content, source, token);
}