using System;

namespace Radish.MonoGame;

internal sealed class GameTimeProvider : ITimeProvider
{
    public TimeSpan TotalElapsed { get; set; }
    public TimeSpan DeltaTime { get; set; }
}