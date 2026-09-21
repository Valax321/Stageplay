using Radish.Platform;
using SDL3;

namespace Radish;

public sealed class StageplayRuntimeBuilder
{
    private StageplayRuntime.StartupInfo StartupInfo;
    
    public Func<StageplayRuntime, IPlatformAchievements>? AchievementsProviderFactory { get; set; }

    private readonly LinkedList<Action<StageplayRuntime>> _initCallbacks = [];

    internal StageplayRuntimeBuilder(StageplayRuntime.StartupInfo info)
    {
        StartupInfo = info;
    }

    public void AddInitCallback(Action<StageplayRuntime> func) => _initCallbacks.AddLast(func);

    public StageplayRuntime Build()
    {
        SDL.SDL_SetAppMetadata(
            StartupInfo.GameInfo.ApplicationName,
            StartupInfo.GameInfo.Version.ToString(3),
            StartupInfo.GameInfo.ApplicationIdentifier
        );

        SDL.SDL_SetAppMetadataProperty(SDL.SDL_PROP_APP_METADATA_CREATOR_STRING, StartupInfo.GameInfo.Organization);
        SDL.SDL_SetAppMetadataProperty(SDL.SDL_PROP_APP_METADATA_TYPE_STRING, "game");

        return new StageplayRuntime(StartupInfo, new StageplayRuntime.PlatformSystemImplementations(AchievementsProviderFactory), _initCallbacks);
    }
}