using Foster.Framework;
using JetBrains.Annotations;

namespace Radish.Content;

/// <summary>
/// Typed caches for runtime data types.
/// </summary>
/// <param name="app"></param>
[PublicAPI]
public class GameResources(StageplayRuntime app)
{
    /// <summary>
    /// Provides a resource cache for textures.
    /// </summary>
    public ResourceProvider<Texture> Textures { get; } = new(app.Content);
}