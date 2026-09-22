using System.Collections.Immutable;
using MemoryPack;

namespace Radish.Scenario;

[MemoryPackable]
public sealed partial class ScenarioGlobals
{
    public required ImmutableDictionary<int, int> State { get; init; }
}