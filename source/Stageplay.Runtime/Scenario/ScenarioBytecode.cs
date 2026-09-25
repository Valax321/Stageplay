using System.Collections.Immutable;
using System.IO.Compression;
using MemoryPack;

namespace Radish.Scenario;

[MemoryPackable]
internal sealed partial class ScenarioBytecode
{
    public required int StartLabelIndex { get; init; }
    
    /// <summary>
    /// The hash is used to detect if a save is loaded with a different scenario version.
    /// If it is, we rewind the VM state to the last visited label instead of jumping to the exact address.
    /// </summary>
    public required uint BytecodeHash { get; init; }
    
    /// <summary>
    /// Map of label string indices to bytecode offsets of the next command.
    /// </summary>
    [BrotliFormatter<ImmutableDictionary<int, int>>(CompressionLevel.SmallestSize)]
    public required ImmutableDictionary<int, int> LabelAddresses { get; init; }
    
    [BrotliFormatter(CompressionLevel.SmallestSize)]
    public required byte[] Data { get; init; }
    
    public required ScenarioSourceMap SourceMap { get; init; }
}