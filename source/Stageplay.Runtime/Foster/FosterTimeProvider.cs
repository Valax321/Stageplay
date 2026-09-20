namespace Radish.Foster;

internal sealed class FosterTimeProvider(StageplayApp app) : ITimeProvider
{
    public TimeSpan TotalElapsed => app.Time.Elapsed;
    public TimeSpan DeltaTime => app.Time.DeltaTimeSpan;
}