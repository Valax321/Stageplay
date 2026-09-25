using MemoryPack;

namespace Radish.Scenario;

[MemoryPackable]
internal sealed partial class ScenarioStringTable
{
    public required string[] Strings { get; init; }
}