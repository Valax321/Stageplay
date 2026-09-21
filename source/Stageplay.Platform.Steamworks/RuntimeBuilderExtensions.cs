using JetBrains.Annotations;
using Steamworks;

namespace Radish.Steamworks;

/// <summary>
/// Steamworks extension methods for <see cref="StageplayRuntimeBuilder"/>.
/// </summary>
[PublicAPI]
public static class RuntimeBuilderExtensions
{
    extension(StageplayRuntimeBuilder builder)
    {
        /// <summary>
        /// Configures the runtime to use the Steamworks API. This must be called before the host is configured, otherwise the host's runtime providers may take priority over the Steam implementations.
        /// </summary>
        /// <remarks>If the Steam API fails to initialise the game will still run, but all API calls will return error values.</remarks>
        /// <param name="appId">The steam app ID to run with.</param>
        /// <param name="callRestartAppIfNecessary">If true, the runtime will call <see cref="SteamClient.RestartAppIfNecessary"/> at startup and terminate if steam requests a restart.</param>
        /// <returns>The input builder instance.</returns>
        public StageplayRuntimeBuilder WithSteamworks(uint appId, bool callRestartAppIfNecessary = true)
        {
            if (callRestartAppIfNecessary && SteamClient.RestartAppIfNecessary(appId))
                Environment.Exit(0);

            try
            {
                SteamClient.Init(appId, asyncCallbacks: false);
            }
            catch (Exception)
            {
                return builder;
            }
            
            builder.AchievementsProviderFactory = _ => new SteamworksAchievements();
            builder.AddInitCallback(runtime =>
            {
                runtime.OnPreUpdate += SteamClient.RunCallbacks;
                runtime.OnShutdown += SteamClient.Shutdown;
            });

            return builder;
        }

        /// <summary>
        /// Configures the runtime to use Steam Input instead of its default input provider.
        /// This needs to be called even if <see cref="RuntimeBuilderExtensions.WithSteamworks"/> is also called,
        /// and must be done before the host is configured (otherwise the host's input provider will take priority).
        /// </summary>
        /// <returns>The input builder instance.</returns>
        public StageplayRuntimeBuilder WithSteamInput()
        {
            return builder;
        }
    }
}