namespace Radish;

/// <summary>
/// Provides access to the game platform's achievement APIs.
/// This interface is not present by default -- an implementation must be provided by an extension package.
/// </summary>
public interface IPlatformAchievements
{
    /// <summary>
    /// A list of achievements available to the platform API.
    /// </summary>
    public IReadOnlyCollection<IAchievement> Achievements { get; }
}