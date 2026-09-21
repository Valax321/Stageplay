using System.Buffers;
using Cysharp.Text;
using JetBrains.Annotations;

namespace Radish.Serialization;

/// <summary>
/// Encodes four ASCII characters into a single 32-bit integer.
/// </summary>
/// <param name="A">The first character.</param>
/// <param name="B">The second character.</param>
/// <param name="C">The third character.</param>
/// <param name="D">The forth character.</param>
[PublicAPI]
public record struct FourCC(byte A, byte B, byte C, byte D) : ISpanFormattable
{
    /// <summary>
    /// Gets the value encoded as an integer.
    /// </summary>
    public uint EncodedValue => ((uint)D << 24) | ((uint)C << 16) | ((uint)B << 8) | A;

    /// <summary>
    /// Reconstructs a FourCC code from its encoded value.
    /// </summary>
    /// <param name="encodedValue"></param>
    public FourCC(uint encodedValue) : this((byte)encodedValue,
        (byte)(encodedValue >> 8),
        (byte)(encodedValue >> 16),
        (byte)(encodedValue >> 24))
    {
    }

    /// <summary>
    /// Creates a FourCC code from a 4 character string.
    /// The characters MUST be ASCII-representable (i.e. less than 256).
    /// </summary>
    /// <param name="code">String with the characters to encode.</param>
    public FourCC(ReadOnlySpan<char> code) : this(StringToFourCc(code))
    {
    }

    private FourCC((byte, byte, byte, byte) b) : this(b.Item1, b.Item2, b.Item3, b.Item4)
    {
    }

    private static (byte, byte, byte, byte) StringToFourCc(ReadOnlySpan<char> str)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(str.Length, 4);

        var a = str[0];
        var b = str[1];
        var c = str[2];
        var d = str[3];

        return ((byte)a, (byte)b, (byte)c, (byte)d);
    }

    /// <inheritdoc/>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        return ToString();
    }

    /// <inheritdoc/>
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format,
        IFormatProvider? provider)
    {
        using var sb = ZString.CreateStringBuilder();
        sb.Append((char)A);
        sb.Append((char)B);
        sb.Append((char)C);
        sb.Append((char)D);
        sb.Write(destination);
        return sb.TryCopyTo(destination, out charsWritten);
    }

    /// <inheritdoc/>
    public readonly override string ToString()
    {
        using var sb = ZString.CreateStringBuilder();
        sb.Append((char)A);
        sb.Append((char)B);
        sb.Append((char)C);
        sb.Append((char)D);
        return sb.ToString();
    }
}