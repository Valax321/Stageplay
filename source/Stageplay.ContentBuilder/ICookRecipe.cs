using JetBrains.Annotations;

namespace Radish.ContentBuilder;

/// <summary>
/// Describes the asset processor steps needed to build a game's assets.
/// </summary>
[PublicAPI]
public interface ICookRecipe
{
    /// <summary>
    /// The task that owns this recipe.
    /// </summary>
    BuilderTask Task { get; init; }

    /// <summary>
    /// Sets up the recipe's processor work.
    /// </summary>
    /// <param name="queue">The queue to encode work into.</param>
    void BuildProcessorQueue(ProcessorQueue queue);
}