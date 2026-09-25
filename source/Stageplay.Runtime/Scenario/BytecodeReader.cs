using JetBrains.Annotations;

namespace Radish.Scenario;

[PublicAPI]
public sealed class BytecodeReader
{
    private readonly byte[] _data;
    private readonly ScenarioStringTable _stringTable;
    
    internal int ReadCount { get; set; }

    internal BytecodeReader(byte[] data, ScenarioStringTable stringTable)
    {
        _data = data;
        _stringTable = stringTable;
    }

    public int Position { get; internal set; }
    public int Length => _data.Length;
    
    public unsafe byte ReadByte()
    {
        var bytes = stackalloc byte[sizeof(byte)];
        var s = new Span<byte>(bytes, sizeof(byte));
        
        ReadByteBlob(s);
        return s[0];
    }
    
    public bool ReadBoolean()
    {
        var val = ReadByte();
        return val > 0;
    }
    
    public unsafe short ReadShort()
    {
        var bytes = stackalloc byte[sizeof(short)];
        var s = new Span<byte>(bytes, sizeof(short));
        
        ReadByteBlob(s);
        return BitConverter.ToInt16(s);
    }

    public unsafe int ReadInteger()
    {
        var bytes = stackalloc byte[sizeof(int)];
        var s = new Span<byte>(bytes, sizeof(int));
        
        ReadByteBlob(s);
        return BitConverter.ToInt32(s);
    }
    
    public unsafe float ReadFloat()
    {
        var bytes = stackalloc byte[sizeof(float)];
        var s = new Span<byte>(bytes, sizeof(float));
        
        ReadByteBlob(s);
        return BitConverter.ToSingle(s);
    }

    public string ReadString()
    {
        var stringIndex = ReadInteger();
        if (stringIndex < 0 || stringIndex >= _stringTable.Strings.Length)
            throw new InvalidDataException($"Out of bounds string index {stringIndex}");

        return _stringTable.Strings[stringIndex];
    }

    public int ReadStringIndex() => ReadInteger();

    private void ReadByteBlob(in Span<byte> dest)
    {
        ReadCount++;
        if (Position + dest.Length > _data.Length)
            throw new EndOfStreamException("Attempted to read out of bounds data from the bytecode blob");
        
        for (var i = 0; i < dest.Length; ++i)
        {
            dest[i] = _data[Position + i];
        }

        Position += dest.Length;
    }
}