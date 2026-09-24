using System.Collections.Immutable;
using System.IO.Compression;
using MemoryPack;

namespace Radish.Scenario;

[MemoryPackable]
public sealed partial class ScenarioGlobals
{
    [BrotliFormatter<ImmutableDictionary<int, int>>(CompressionLevel.SmallestSize)]
    public required ImmutableDictionary<int, int> State { get; init; }
}