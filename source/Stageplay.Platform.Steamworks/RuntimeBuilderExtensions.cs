using JetBrains.Annotations;
using SDL3;
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
        /// <param name="errorIfSteamInitFailed">If true, then a failure to initialise the steam API will show an error dialog and then quit the application.</param>
        /// <returns>The input builder instance.</returns>
        public StageplayRuntimeBuilder WithSteamworks(uint appId, bool callRestartAppIfNecessary = true, bool errorIfSteamInitFailed = false)
        {
            // We can't use The CommandLine parser the runtime has since this is called before that is created.
            if (Environment.GetCommandLineArgs().Contains("-nosteam", StringComparer.InvariantCultureIgnoreCase))
            {
                errorIfSteamInitFailed = false;
                callRestartAppIfNecessary = false;
            }
            
            if (callRestartAppIfNecessary && SteamClient.RestartAppIfNecessary(appId))
                Environment.Exit(0);

            try
            {
                SteamClient.Init(appId, asyncCallbacks: false);
            }
            catch (Exception ex)
            {
                if (errorIfSteamInitFailed)
                {
                    SDL.SDL_ShowSimpleMessageBox(SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR,
                        "Steam Initialization Failed", ex.Message, 0);
                    Environment.Exit(1);
                }
                
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