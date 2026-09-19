namespace Radish;

/// <summary>
/// Provides access to the runtime host's timing info.
/// </summary>
public interface ITimeProvider
{
    /// <summary>
    /// The time elapsed since the game started.
    /// </summary>
    public TimeSpan TotalElapsed { get; }
    
    /// <summary>
    /// The time elapsed since the previous frame.
    /// </summary>
    TimeSpan DeltaTime { get; }
}
