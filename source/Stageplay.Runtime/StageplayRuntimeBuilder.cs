using Radish.Platform;
using SDL3;

namespace Radish;

/// <summary>
/// Builder API for customising a new runtime instance.
/// </summary>
public sealed class StageplayRuntimeBuilder
{
    /// <summary>
    /// Provides the runtime a factory method for creating a platform-specific achievement implementation.
    /// </summary>
    public Func<StageplayRuntime, IPlatformAchievements>? AchievementsProviderFactory { get; set; }

    private readonly LinkedList<Action<StageplayRuntime>> _initCallbacks = [];
    private readonly StageplayRuntime.StartupInfo _startupInfo;

    internal StageplayRuntimeBuilder(StageplayRuntime.StartupInfo info)
    {
        _startupInfo = info;
    }

    /// <summary>
    /// Adds a method to be called when the runtime finishes initialising.
    /// </summary>
    /// <param name="func"></param>
    public void AddInitCallback(Action<StageplayRuntime> func) => _initCallbacks.AddLast(func);

    /// <summary>
    /// Builds the actual <see cref="StageplayRuntime"/> from this builder's configuration.
    /// </summary>
    /// <returns>The constructed runtime.</returns>
    public StageplayRuntime Build()
    {
        SDL.SDL_SetAppMetadata(
            _startupInfo.GameInfo.ApplicationName,
            _startupInfo.GameInfo.Version.ToString(3),
            _startupInfo.GameInfo.ApplicationIdentifier
        );

        SDL.SDL_SetAppMetadataProperty(SDL.SDL_PROP_APP_METADATA_CREATOR_STRING, _startupInfo.GameInfo.Organization);
        SDL.SDL_SetAppMetadataProperty(SDL.SDL_PROP_APP_METADATA_TYPE_STRING, "game");

        return new StageplayRuntime(_startupInfo, new StageplayRuntime.PlatformSystemImplementations(AchievementsProviderFactory), _initCallbacks);
    }
}