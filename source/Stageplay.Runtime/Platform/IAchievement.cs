namespace Radish.Platform;

/// <summary>
/// Interface to a single achievement.
/// The actual implementation is platform-specific.
/// </summary>
public interface IAchievement
{
    /// <summary>
    /// The internal name of the achievement.
    /// This may differ between platforms, and is usually what you set up at a platform API level (e.g. on your Steamworks app page).
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// If true, this achievement is already unlocked.
    /// </summary>
    bool Unlocked { get; }

    /// <summary>
    /// Unlocks this achievement.
    /// If the achievement is already unlocked, nothing happens.
    /// </summary>
    /// <remarks>This can be asynchronous in the background on some platforms, so it is not recommended to rely on the <see cref="Unlocked"/> status being immediately updated.</remarks>
    public void Unlock();
}