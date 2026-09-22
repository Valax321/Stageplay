using System.Collections.Immutable;
using System.IO.Compression;
using MemoryPack;

namespace Radish.Scenario;

[MemoryPackable]
public sealed partial class ScenarioBytecode
{
    /// <summary>
    /// Map of label string indices to bytecode offsets of the next command.
    /// </summary>
    public required ImmutableDictionary<int, int> LabelAddresses { get; init; }
    
    /// <summary>
    /// Map of bytecode offsets to source code locations.
    /// </summary>
    public required ImmutableDictionary<int, DebuggerLocation> SourceMap { get; init; }
    
    /// <summary>
    /// The hash is used to detect if a save is loaded with a different scenario version.
    /// If it is, we rewind the VM state to the last visited label instead of jumping to the exact address.
    /// </summary>
    public required uint BytecodeHash { get; init; }
    
    [BrotliFormatter(CompressionLevel.SmallestSize)]
    public required byte[] Bytecode { get; init; }
}