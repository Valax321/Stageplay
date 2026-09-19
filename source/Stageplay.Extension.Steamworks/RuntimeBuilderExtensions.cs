using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
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
        /// Configures the runtime to use the Steamworks API.
        /// Note that if the Steam API fails to initialise the game will still run, but all API calls will return error values.
        /// </summary>
        /// <param name="appId">The steam app ID to run with.</param>
        /// <param name="callRestartAppIfNecessary">If true, the runtime will call <see cref="SteamClient.RestartAppIfNecessary"/> at startup and terminate if steam requests a restart.</param>
        /// <returns>The same builder instance.</returns>
        public StageplayRuntimeBuilder WithSteamworks(uint appId, bool callRestartAppIfNecessary = true)
        {
            builder.Services.AddSingleton<IPlatformAchievements, SteamworksAchievements>();
            
            builder.WithConfigure(runtime =>
            {
                if (callRestartAppIfNecessary && SteamClient.RestartAppIfNecessary(appId))
                    Environment.Exit(0);

                try
                {
                    SteamClient.Init(appId, asyncCallbacks: false);
                }
                catch (Exception)
                {
                    return;
                }

                runtime.Host.OnUpdate += SteamClient.RunCallbacks;
                runtime.Host.OnShutdown += SteamClient.Shutdown;
            });

            return builder;
        }
    }
}
