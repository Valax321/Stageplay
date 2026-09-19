using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

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
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TGame>()
            where TGame : StageplayGame
        {
            builder.Services.AddSingleton<IStageplaySystemHost, TGame>();
            builder.AddSingletonFromGame(g => g.AudioProvider);
            builder.AddSingletonFromGame(g => g.TimeProvider);

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
        public void AddSingletonFromGame<T>(Func<StageplayGame, T> resolver) where T : class
        {
            builder.Services.AddSingleton<T>(s =>
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