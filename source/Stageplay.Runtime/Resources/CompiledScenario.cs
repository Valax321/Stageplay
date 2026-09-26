using JetBrains.Annotations;
using MemoryPack;
using Radish.Content;
using Radish.Scenario;
using Radish.Serialization;

namespace Radish.Resources;

[PublicAPI]
[MemoryPackable]
public sealed partial class CompiledScenario : IBinarySerializable
{
    public static FourCC HeaderMagic => new("SCNR");
    
    [MemoryPackInclude]
    internal ScenarioGlobals Globals { get; }
    
    [MemoryPackInclude]
    internal ScenarioBytecode Bytecode { get; }
    
    [MemoryPackInclude]
    internal ScenarioStringTable StringTable { get; }
    
    [MemoryPackIgnore] 
    public string StartLabel => GetStringByIndex(Bytecode.StartLabelIndex);

    [MemoryPackConstructor]
    internal CompiledScenario(ScenarioGlobals globals, ScenarioBytecode bytecode, ScenarioStringTable stringTable)
    {
        Globals = globals;
        Bytecode = bytecode;
        StringTable = stringTable;
    }

    public string GetStringByIndex(int stringIndex)
    {
        return StringTable.Strings[stringIndex];
    }

    public int? FindLabelAddressByName(string name)
    {
        var labelIndex = StringTable.Strings.IndexOf(name);
        if (labelIndex < 0)
            return null;

        if (!Bytecode.LabelAddresses.TryGetValue(labelIndex, out var address))
            return null;

        return address;
    }

    public SourceLocation? GetSourceLocation(int address)
    {
        if (!Bytecode.SourceMap.SourceMap.TryGetValue(address, out var loc))
            return null;

        return loc;
    }

    public static CompiledScenario Load(Stream source)
    {
        return BinaryObject.FromStream<CompiledScenario>(source);
    }
    
    public static ValueTask<CompiledScenario> LoadAsync(Stream source)
    {
        return BinaryObject.FromStreamAsync<CompiledScenario>(source);
    }
}