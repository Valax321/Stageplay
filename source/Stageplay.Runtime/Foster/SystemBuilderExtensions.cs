using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Lua.Platforms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Radish.Foster.Lua;
using Radish.Lua;
using SDL3;

namespace Radish.Foster;

/// <summary>
/// <see cref="StageplayRuntimeBuilder"/> extensions for Foster.
/// </summary>
[PublicAPI]
public static class SystemBuilderExtensions
{
    extension(StageplayRuntimeBuilder builder)
    {
        /// <summary>
        /// Configures the runtime to use a custom MonoGame host class.
        /// </summary>
        /// <typeparam name="TGame">The custom <see cref="StageplayApp"/> class to run with. The class must have a valid constructor that is supported by dependency injection.</typeparam>
        /// <returns>The same builder instance.</returns>
        public StageplayRuntimeBuilder WithFosterHost<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
            TGame>()
            where TGame : StageplayApp
        {
            // Add the host itself
            builder.Services.AddSingleton<IStageplaySystemHost, TGame>();

            // Core subsystems that provide high-level engine components.
            builder.TryAddSingletonFromApp(g => g.AudioProvider);
            builder.TryAddSingletonFromApp(g => g.TimeProvider);
            builder.TryAddSingletonFromApp(g => g.Resources);

            // These are required for the lua VM to work
            builder.TryAddSingletonFromApp(g => new LuaPlatform(
                new FosterLuaFilesystem(g),
                new FosterOsEnvironment(g),
                new FosterLuaStandardIO(),
                TimeProvider.System
            ));
            builder.TryAddSingletonFromApp<ILuaModuleLoaderSync>(g => new FosterLuaModuleLoader(g));

            // Set up the app metadata prior to the host being created
            builder.WithConfigure(runtime =>
            {
                var gameInfo = runtime.Services.GetRequiredService<GameInfo>();
                
                SDL.SDL_SetAppMetadata(
                    gameInfo.ApplicationName, 
                    gameInfo.Version.ToString(3),
                    gameInfo.ApplicationIdentifier
                );

                SDL.SDL_SetAppMetadataProperty(SDL.SDL_PROP_APP_METADATA_CREATOR_STRING, gameInfo.Organization);
                SDL.SDL_SetAppMetadataProperty(SDL.SDL_PROP_APP_METADATA_TYPE_STRING, "game");
            });

            return builder;
        }

        /// <summary>
        /// Configures the runtime to use the default MonoGame host class.
        /// </summary>
        /// <returns>The same builder instance.</returns>
        public StageplayRuntimeBuilder WithFosterHost()
            => builder.WithFosterHost<StageplayApp>();

        /// <summary>
        /// Shortcut to add a singleton service from the MonoGame game instance used as the runtime host.
        /// </summary>
        /// <param name="resolver">Method to resolve the service from the game.</param>
        /// <typeparam name="T">The type of service to resolve.</typeparam>
        public void TryAddSingletonFromApp<T>(Func<StageplayApp, T> resolver) where T : class
        {
            builder.Services.TryAddSingleton<T>(s =>
            {
                var host = s.GetRequiredService<IStageplaySystemHost>();
                if (host is not StageplayApp game)
                    throw new InvalidOperationException(
                        "Stageplay runtime host was not a Foster app. Did you remember to call WithFosterHost() on your runtime builder?");

                return resolver(game);
            });
        }
    }
}