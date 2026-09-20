using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Lua.Platforms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Radish.Lua;
using Radish.MonoGame.Lua;

namespace Radish.MonoGame;

/// <summary>
/// MonoGame extension methods for <see cref="StageplayRuntimeBuilder"/>.
/// </summary>
[PublicAPI]
public static class SystemBuilderExtensions
{
    extension(StageplayRuntimeBuilder builder)
    {
        /// <summary>
        /// Configures the runtime to use a custom MonoGame host class.
        /// </summary>
        /// <typeparam name="TGame">The custom <see cref="StageplayGame"/> class to run with. The class must have a valid constructor that is supported by dependency injection.</typeparam>
        /// <returns>The same builder instance.</returns>
        public StageplayRuntimeBuilder WithMonoGameHost<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
            TGame>()
            where TGame : StageplayGame
        {
            // Add the host itself
            builder.Services.AddSingleton<IStageplaySystemHost, TGame>();

            // Core subsystems that provide high-level engine components.
            builder.TryAddSingletonFromGame(g => g.AudioProvider);
            builder.TryAddSingletonFromGame(g => g.TimeProvider);
            builder.TryAddSingletonFromGame(g => g.Resources);

            // These are required for the lua VM to work
            builder.TryAddSingletonFromGame(g => new LuaPlatform(
                new GameLuaFilesystem(),
                new GameLuaOSEnvironment(g),
                new GameLuaStandardIO(),
                TimeProvider.System
            ));
            builder.TryAddSingletonFromGame<ILuaModuleLoaderSync>(g => new GameLuaModuleLoader(g));

            return builder;
        }

        /// <summary>
        /// Configures the runtime to use the default MonoGame host class.
        /// </summary>
        /// <returns>The same builder instance.</returns>
        public StageplayRuntimeBuilder WithMonoGameHost()
            => builder.WithMonoGameHost<StageplayGame>();

        /// <summary>
        /// Shortcut to add a singleton service from the MonoGame game instance used as the runtime host.
        /// </summary>
        /// <param name="resolver">Method to resolve the service from the game.</param>
        /// <typeparam name="T">The type of service to resolve.</typeparam>
        public void TryAddSingletonFromGame<T>(Func<StageplayGame, T> resolver) where T : class
        {
            builder.Services.TryAddSingleton<T>(s =>
            {
                var host = s.GetRequiredService<IStageplaySystemHost>();
                if (host is not StageplayGame game)
                    throw new InvalidOperationException(
                        "Stageplay runtime host was not a MonoGame game. Did you remember to call WithMonoGameHost() on your runtime builder?");

                return resolver(game);
            });
        }
    }
}