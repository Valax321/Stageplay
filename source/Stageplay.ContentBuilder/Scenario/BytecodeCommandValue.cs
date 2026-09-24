using JetBrains.Annotations;

namespace Radish.ContentBuilder.Scenario;

/// <summary>
/// Base class for bytecode arguments.
/// </summary>
public abstract class BytecodeCommandValue
{
    internal abstract byte[] ToBytes(BytecodeWriterState state);
}

/// <summary>
/// String bytecode argument.
/// </summary>
[PublicAPI]
public sealed class BytecodeString(string value) : BytecodeCommandValue
{
    internal override byte[] ToBytes(BytecodeWriterState state)
    {
        var stringIndex = state.Context.StringTable.GetUniqueStringIndex(value);
        return BitConverter.GetBytes(stringIndex);
    }
}

/// <summary>
/// Byte bytecode argument.
/// </summary>
[PublicAPI]
public sealed class BytecodeByte(byte value) : BytecodeCommandValue
{
    internal override byte[] ToBytes(BytecodeWriterState state)
    {
        return [value];
    }
}

/// <summary>
/// Bytecode boolean argument.
/// </summary>
[PublicAPI]
public sealed class BytecodeBoolean(bool value) : BytecodeCommandValue
{
    internal override byte[] ToBytes(BytecodeWriterState state)
    {
        return [(byte)(value ? 1 : 0)];
    }
}

/// <summary>
/// Bytecode short argument.
/// </summary>
public sealed class BytecodeShort(short value) : BytecodeCommandValue
{
    internal override byte[] ToBytes(BytecodeWriterState state)
    {
        return BitConverter.GetBytes(value);
    }
}

/// <summary>
/// Bytecode int argument.
/// </summary>
[PublicAPI]
public sealed class BytecodeInteger(int value) : BytecodeCommandValue
{
    internal override byte[] ToBytes(BytecodeWriterState state)
    {
        return BitConverter.GetBytes(value);
    }
}

/// <summary>
/// Bytecode float argument.
/// </summary>
[PublicAPI]
public sealed class BytecodeFloat(float value) : BytecodeCommandValue
{
    internal override byte[] ToBytes(BytecodeWriterState state)
    {
        return BitConverter.GetBytes(value);
    }
}