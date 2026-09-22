using JetBrains.Annotations;

namespace Radish.ContentBuilder.Scenario;

public abstract class BytecodeCommandValue
{
    internal abstract byte[] ToBytes(BytecodeWriterState state);
}

[PublicAPI]
public sealed class BytecodeString(string value) : BytecodeCommandValue
{
    internal override byte[] ToBytes(BytecodeWriterState state)
    {
        var stringIndex = state.Context.StringTable.GetUniqueStringIndex(value);
        return BitConverter.GetBytes(stringIndex);
    }
}

[PublicAPI]
public sealed class BytecodeByte(byte value) : BytecodeCommandValue
{
    internal override byte[] ToBytes(BytecodeWriterState state)
    {
        return [value];
    }
}

[PublicAPI]
public sealed class BytecodeBoolean(bool value) : BytecodeCommandValue
{
    internal override byte[] ToBytes(BytecodeWriterState state)
    {
        return [(byte)(value ? 1 : 0)];
    }
}

public sealed class BytecodeShort(short value) : BytecodeCommandValue
{
    internal override byte[] ToBytes(BytecodeWriterState state)
    {
        return BitConverter.GetBytes(value);
    }
}

[PublicAPI]
public sealed class BytecodeInteger(int value) : BytecodeCommandValue
{
    internal override byte[] ToBytes(BytecodeWriterState state)
    {
        return BitConverter.GetBytes(value);
    }
}

[PublicAPI]
public sealed class BytecodeFloat(float value) : BytecodeCommandValue
{
    internal override byte[] ToBytes(BytecodeWriterState state)
    {
        return BitConverter.GetBytes(value);
    }
}