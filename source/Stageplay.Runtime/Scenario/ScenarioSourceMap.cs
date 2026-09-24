using System.Collections.Immutable;
using System.IO.Compression;
using JetBrains.Annotations;
using MemoryPack;
using Radish.Serialization;

namespace Radish.Scenario;

[PublicAPI]
[MemoryPackable]
public sealed partial class ScenarioSourceMap : IBinarySerializable
{
    public static FourCC HeaderMagic => new("SSRC");

    /// <summary>
    /// Map of bytecode offsets to source code locations.
    /// </summary>
    [BrotliFormatter<ImmutableDictionary<int, DebuggerLocation>>(CompressionLevel.SmallestSize)]
    public required ImmutableDictionary<int, DebuggerLocation> SourceMap { get; init; }
}