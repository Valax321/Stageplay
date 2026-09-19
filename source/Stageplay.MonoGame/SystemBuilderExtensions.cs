using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Radish.MonoGame;

[PublicAPI]
public static class SystemBuilderExtensions
{
    extension(StageplayRuntimeBuilder builder)
    {
        public StageplayRuntimeBuilder WithMonoGameHost<TGame>() 
            where TGame : StageplayGame
        {
            builder.Services.AddSingleton<IStageplaySystemHost, StageplayGame>();
            builder.AddFromGame(g => g.AudioProvider);
            builder.AddFromGame(g => g.TimeProvider);

            return builder;
        }

        public StageplayRuntimeBuilder WithMonoGameHost() 
            => builder.WithMonoGameHost<StageplayGame>();

        public void AddFromGame<T>(Func<StageplayGame, T> resolver) where T : class
        {
            builder.Services.AddSingleton<T>(s =>
            {
                var game = (StageplayGame)s.GetRequiredService<IStageplaySystemHost>();
                return resolver(game);
            });
        }
    }
}