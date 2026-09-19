using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Steamworks;

namespace Radish.Steamworks;

[PublicAPI]
public static class RuntimeBuilderExtensions
{
    extension(StageplayRuntimeBuilder builder)
    {
        public StageplayRuntimeBuilder WithSteamworks(uint appId, bool callRestartAppIfNecessary = true)
        {
            builder.Services.AddSingleton<IPlatformAchievements, SteamworksAchievements>();
            
            builder.ConfigureWith(runtime =>
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
