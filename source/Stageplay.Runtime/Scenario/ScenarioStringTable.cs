using MemoryPack;

namespace Radish.Scenario;

[MemoryPackable]
public sealed partial class ScenarioStringTable
{
    public required string[] Strings { get; init; }
}