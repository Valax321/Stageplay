namespace Radish;

public interface ITimeProvider
{
    public TimeSpan TotalElapsed { get; }
    TimeSpan DeltaTime { get; }
}
