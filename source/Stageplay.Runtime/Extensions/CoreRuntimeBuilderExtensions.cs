using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Radish;

[PublicAPI]
public static class CoreRuntimeBuilderExtensions
{
    extension(StageplayRuntimeBuilder builder)
    {
        public StageplayRuntimeBuilder WithGameInfo(GameInfo gi)
        {
            builder.Services.AddSingleton<GameInfo>(gi);
            return builder;
        }
    }
}