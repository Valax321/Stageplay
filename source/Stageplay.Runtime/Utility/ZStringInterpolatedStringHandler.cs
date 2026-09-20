using System.Runtime.CompilerServices;
using Cysharp.Text;

namespace Radish.Utility;

/// <summary>
/// String interpolation handler backed by <see cref="ZString"/>, so it doesn't allocate on the heap.
/// </summary>
[InterpolatedStringHandler]
public ref struct ZStringInterpolatedStringHandler : IDisposable, ISinkWriter
{
    private Utf16ValueStringBuilder _sb;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="literalLength"></param>
    /// <param name="formattedCount"></param>
    public ZStringInterpolatedStringHandler(int literalLength, int formattedCount)
    {
        _sb = ZString.CreateStringBuilder();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="s"></param>
    public void AppendLiteral(ReadOnlySpan<char> s)
    {
        _sb.Append(s);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="val"></param>
    /// <typeparam name="T"></typeparam>
    public void AppendFormatted<T>(T val)
    {
        _sb.Append(val);
    }

    /// <inheritdoc/>
    public ReadOnlySpan<char> AsSpan()
    {
        return _sb.AsSpan();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return _sb.ToString();
    }
        
    /// <inheritdoc/>
    public void Dispose()
    {
        _sb.Dispose();
    }
}