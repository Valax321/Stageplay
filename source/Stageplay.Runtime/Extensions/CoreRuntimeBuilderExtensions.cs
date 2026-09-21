using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Lua.IO;
using Lua.Platforms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Radish.Audio;
using Radish.Content;
using Radish.Impl;
using Radish.Lua;
using Radish.Lua.Impl;
using SDL3;

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

        /// <summary>
        /// Configures the runtime to use a custom game class.
        /// </summary>
        /// <typeparam name="TGame">The custom <see cref="Game"/> class to run with. The class must have a valid constructor that is supported by dependency injection.</typeparam>
        /// <returns>The same builder instance.</returns>
        public StageplayRuntimeBuilder WithCoreSystemComponents<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
            TGame>()
            where TGame : Game
        {
            // Add the host itself
            builder.Services.AddSingleton<IStageplayRuntime, StageplayRuntime>();
            builder.Services.AddSingleton<Game, TGame>();
            builder.Services.AddSingleton<IRuntimeLifecycleHooks, LifecycleHooksImpl>();

            // Core subsystems that provide high-level engine components.
            builder.Services.TryAddSingleton<IAudioProvider, NullAudioProvider>();
            builder.Services.TryAddSingleton<ITimeProvider, TimeProviderImpl>();
            builder.Services.TryAddSingleton<IResourcesProvider, ResourceProviderImpl>();
            builder.Services.TryAddSingleton<IContentManager, ContentManager>();

            // These are required for the lua VM to work
            builder.Services.TryAddSingleton<ILuaFileSystem, FosterLuaFilesystem>();
            builder.Services.TryAddSingleton<ILuaOsEnvironment, FosterOsEnvironment>();
            builder.Services.TryAddSingleton<ILuaStandardIO, FosterLuaStandardIO>();
            builder.Services.TryAddSingleton<ILuaModuleLoaderSync, FosterLuaModuleLoader>();

            builder.Services.TryAddSingleton(services => new LuaPlatform(
                services.GetRequiredService<ILuaFileSystem>(),
                services.GetRequiredService<ILuaOsEnvironment>(),
                services.GetRequiredService<ILuaStandardIO>(),
                TimeProvider.System
            ));

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
        /// Configures the runtime to use the default game class.
        /// </summary>
        /// <returns>The same builder instance.</returns>
        public StageplayRuntimeBuilder WithCoreSystemComponents()
            => builder.WithCoreSystemComponents<DefaultGameImpl>();
    }
}