using JetBrains.Annotations;
using MemoryPack;
using Radish.Scenario;
using Radish.Serialization;

namespace Radish.Resources;

[PublicAPI]
[MemoryPackable]
public sealed partial class CompiledScenario : IBinarySerializable
{
    public static FourCC HeaderMagic => new("SCNR");

    public required int StartLabel { get; init; }
    
    public required ScenarioGlobals Globals { get; init; }
    
    public required ScenarioBytecode Bytecode { get; init; }
    
    public required ScenarioStringTable StringTable { get; init; }
}