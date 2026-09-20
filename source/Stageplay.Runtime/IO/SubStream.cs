namespace Radish.IO;

/// <summary>
/// Wraps another stream to provide only a subset of its contents.
/// </summary>
/// <param name="owner">The stream to wrap.</param>
/// <param name="offset">The starting offset in the stream to allow access to.</param>
/// <param name="length">The maximum length of the stream to allow access to.</param>
/// <param name="leaveOpen">If true <paramref name="owner"/> will be left open when this stream is disposed.</param>
public sealed class SubStream(Stream owner, long offset, long length, bool leaveOpen = false) : Stream
{
    /// <inheritdoc/>
    public override bool CanRead => owner.CanRead;
    
    /// <inheritdoc/>
    public override bool CanSeek => owner.CanSeek;
    
    /// <inheritdoc/>
    public override bool CanWrite => owner.CanWrite;
    
    /// <inheritdoc/>
    public override long Length => length;

    /// <inheritdoc/>
    public override long Position
    {
        get => owner.Position - offset;
        set
        {
            owner.Position = offset + value;
            
            if (owner.Position < offset)
                owner.Position = offset;

            if (owner.Position >= offset + length)
                owner.Position = owner.Position = (offset + length) - 1;
        }
    }
    
    /// <inheritdoc/>
    public override void Flush()
    {
        owner.Flush();
    }

    /// <inheritdoc/>
    public override int Read(byte[] buffer, int o, int count)
    {
        var wantsWritePos = owner.Position + count;
        var canHaveWritePos = offset + length - 1;

        if (wantsWritePos > canHaveWritePos)
            count -= (int)(wantsWritePos - canHaveWritePos);
        
        return owner.Read(buffer, o, count);
    }

    /// <inheritdoc/>
    public override long Seek(long o, SeekOrigin origin)
    {
        //FIXME: this needs bounds checking
        var newPos = origin switch
        {
            SeekOrigin.Begin => owner.Seek(offset + o, origin),
            SeekOrigin.Current => owner.Seek(o, SeekOrigin.Current),
            SeekOrigin.End => owner.Seek(offset + (length - o), SeekOrigin.Begin),
            _ => throw new ArgumentOutOfRangeException(nameof(origin), origin, null)
        };

        if (newPos < offset)
            newPos = owner.Position = offset;

        if (newPos >= offset + length)
            newPos = owner.Position = (offset + length) - 1;
        
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
        var wantsWritePos = owner.Position + count;
        var canHaveWritePos = offset + length - 1;

        if (wantsWritePos > canHaveWritePos)
            count -= (int)(wantsWritePos - canHaveWritePos);
        
        owner.Write(buffer, o, count);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing && !leaveOpen)
            owner.Dispose();
    }
}