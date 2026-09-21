namespace Radish.Impl;

internal sealed class TimeProviderImpl(StageplayRuntime app) : ITimeProvider
{
    public TimeSpan TotalElapsed => app.Time.Elapsed;
    public TimeSpan DeltaTime => app.Time.DeltaTimeSpan;
}