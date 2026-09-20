namespace Radish.IO;

/// <summary>
/// Wraps another stream to provide only a subset of its contents.
/// </summary>
public sealed class SubStream : Stream
{
    private readonly Stream _owner;
    private readonly long _offset;
    private readonly long _length;
    private readonly bool _leaveOpen;

    /// <summary>
    /// Wraps another stream to provide only a subset of its contents.
    /// </summary>
    /// <param name="owner">The stream to wrap.</param>
    /// <param name="offset">The starting offset in the stream to allow access to.</param>
    /// <param name="length">The maximum length of the stream to allow access to.</param>
    /// <param name="leaveOpen">If true <paramref name="owner"/> will be left open when this stream is disposed.</param>
    public SubStream(Stream owner, long offset, long length, bool leaveOpen = false)
    {
        _owner = owner;
        _offset = offset;
        _length = length;
        _leaveOpen = leaveOpen;

        owner.Seek(_offset, SeekOrigin.Begin);
    }

    /// <inheritdoc/>
    public override bool CanRead => _owner.CanRead;
    
    /// <inheritdoc/>
    public override bool CanSeek => _owner.CanSeek;
    
    /// <inheritdoc/>
    public override bool CanWrite => _owner.CanWrite;
    
    /// <inheritdoc/>
    public override long Length => _length;

    /// <inheritdoc/>
    public override long Position
    {
        get => _owner.Position - _offset;
        set
        {
            _owner.Position = _offset + value;
            
            if (_owner.Position < _offset)
                _owner.Position = _offset;

            if (_owner.Position > _offset + _length)
                _owner.Position = _offset + _length;
        }
    }
    
    /// <inheritdoc/>
    public override void Flush()
    {
        _owner.Flush();
    }

    /// <inheritdoc/>
    public override int Read(byte[] buffer, int o, int count)
    {
        var wantsWritePos = _owner.Position + count;
        var endPos = _offset + _length;

        if (wantsWritePos > endPos)
            count -= (int)(wantsWritePos - endPos);
        
        return _owner.Read(buffer, o, count);
    }

    /// <inheritdoc/>
    public override long Seek(long o, SeekOrigin origin)
    {
        //FIXME: this needs bounds checking
        var newPos = origin switch
        {
            SeekOrigin.Begin => _owner.Seek(_offset + o, origin),
            SeekOrigin.Current => _owner.Seek(o, SeekOrigin.Current),
            SeekOrigin.End => _owner.Seek(_offset + (_length - o), SeekOrigin.Begin),
            _ => throw new ArgumentOutOfRangeException(nameof(origin), origin, null)
        };

        if (newPos < _offset)
            newPos = _owner.Position = _offset;

        if (newPos > _offset + _length)
            newPos = _owner.Position = _offset + _length;
        
        return newPos;
    }

    /// <inheritdoc/>
    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc/>
    public override void Write(byte[] buffer, int o, int count)
    {
        var wantsReadPos = _owner.Position + count;
        var endPos = _offset + _length;

        if (wantsReadPos > endPos)
            count -= (int)(wantsReadPos - endPos);
        
        _owner.Write(buffer, o, count);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing && !_leaveOpen)
            _owner.Dispose();
    }
}