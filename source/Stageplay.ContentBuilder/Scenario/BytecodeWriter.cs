using System.Collections.Immutable;
using System.IO.Hashing;
using Radish.Scenario;

namespace Radish.ContentBuilder.Scenario;

internal record BytecodeWriterState(
    BytecodeWriter Context,
    Stream Output,
    IDictionary<int, int> LabelOffsetTable,
    IDictionary<int, SourceLocation> SourceMap
);

internal interface IBytecodeRecord
{
    void WriteBytes(BytecodeWriterState state);
}

internal record Label(string LabelName) : IBytecodeRecord
{
    public void WriteBytes(BytecodeWriterState state)
    {
        state.LabelOffsetTable.Add(state.Context.StringTable.GetUniqueStringIndex(LabelName),
            (int)state.Output.Position);
    }
}

internal record CommandPacket(
    string CommandName,
    (string File, int Line) DebugInfo,
    params IReadOnlyList<BytecodeCommandValue> Arguments)
    : IBytecodeRecord
{
    public void WriteBytes(BytecodeWriterState state)
    {
        var debugInfoFileStringIndex = state.Context.StringTable.GetUniqueStringIndex(DebugInfo.File);
        state.SourceMap.Add((int)state.Output.Position, new SourceLocation(debugInfoFileStringIndex, DebugInfo.Line));

        var commandNameStringIndex = state.Context.StringTable.GetUniqueStringIndex(CommandName);
        var argCount = (byte)Arguments.Count;
        
        state.Output.Write(BitConverter.GetBytes(commandNameStringIndex));
        state.Output.Write(new ReadOnlySpan<byte>(ref argCount));
        
        foreach (var arg in Arguments)
            state.Output.Write(arg.ToBytes(state));
    }
}

internal sealed class BytecodeWriter
{
    public WritableStringTable StringTable { get; }

    private readonly List<IBytecodeRecord> _records = [];

    public BytecodeWriter(WritableStringTable stringTable)
    {
        StringTable = stringTable;
    }

    public void WriteCommandPacket(CommandPacket packet)
    {
        _records.Add(packet);
    }

    public void WriteLabel(string labelName)
    {
        _records.Add(new Label(labelName));
    }

    public ScenarioBytecode Compile(int entrypointIndex)
    {
        using var bc = new MemoryStream();

        // Maps string index => label ID
        var labelIndices = new Dictionary<int, int>();

        // Maps label ID => address
        var labelOffsets = new Dictionary<int, int>();

        // Maps address => (file string index, line)
        var sourceMap = new Dictionary<int, SourceLocation>();

        foreach (var (i, label) in _records.OfType<Label>().Index())
        {
            var stringTableIndex = StringTable.GetUniqueStringIndex(label.LabelName);
            labelIndices.Add(stringTableIndex, i);
        }

        var state = new BytecodeWriterState(this, bc, labelOffsets, sourceMap);
        foreach (var rec in _records)
        {
            rec.WriteBytes(state);
        }

        var bytecodeArray = bc.ToArray();
        var hash = XxHash32.HashToUInt32(bytecodeArray);

        var rbc = new ScenarioBytecode
        {
            LabelAddresses = labelIndices.ToImmutableDictionary(),
            Data = bytecodeArray,
            BytecodeHash = hash,
            StartLabelIndex = entrypointIndex,
            SourceMap = new ScenarioSourceMap
            {
                SourceMap = sourceMap.ToImmutableDictionary()
            }
        };

        return rbc;
    }
}