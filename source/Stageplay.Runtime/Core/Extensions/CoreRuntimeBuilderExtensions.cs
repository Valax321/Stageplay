using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Radish;

/// <summary>
/// Extension methods for <see cref="StageplayRuntimeBuilder"/>.
/// </summary>
[PublicAPI]
public static class CoreRuntimeBuilderExtensions
{
    extension(StageplayRuntimeBuilder builder)
    {
        /// <summary>
        /// Sets the game info record for the runtime.
        /// </summary>
        /// <param name="gi">The game info to run with.</param>
        /// <returns>The same builder instance, for fluent API.</returns>
        public StageplayRuntimeBuilder WithGameInfo(GameInfo gi)
        {
            builder.Services.AddSingleton<GameInfo>(gi);
            return builder;
        }
    }
}