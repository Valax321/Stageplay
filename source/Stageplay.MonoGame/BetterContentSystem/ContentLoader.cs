using Microsoft.Xna.Framework.Content;

namespace Radish.MonoGame.BetterContentSystem;

/// <summary>
/// <see cref="ContentTypeReader{T}"/> but good.
/// </summary>
public abstract class ContentLoader
{
    protected internal ContentManager Content { get; set; } = null!;

    /// <summary>
    /// The extension for the cooked asset.
    /// </summary>
    public abstract string GetFileExtension(string originalPath);
    
    /// <summary>
    /// Loads the asset object from the given stream.
    /// </summary>
    /// <param name="source">The stream to load from.</param>
    /// <returns>The loaded asset.</returns>
    public abstract object Load(Stream source);
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
    protected abstract T LoadTyped(Stream source);

    /// <inheritdoc/>
    public override object Load(Stream source) 
        => LoadTyped(source);
}