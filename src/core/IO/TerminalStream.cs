namespace Vezel.Cathode.IO;

[SuppressMessage("", "CA1844")]
public abstract class TerminalStream : Stream
{
    // This class and classes inheriting from it should not override Close, Dispose, or DisposeAsync.

    public override sealed bool CanSeek => false;

    public override sealed long Length => throw new NotSupportedException();

    public override sealed long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    private protected TerminalStream()
    {
    }

    public override sealed long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    public override sealed void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override sealed void Flush()
    {
    }

    public override sealed int Read(byte[] buffer, int offset, int count)
    {
        ValidateBufferArguments(buffer, offset, count);

        return Read(buffer.AsSpan(offset..count));
    }

    public override sealed Task<int> ReadAsync(
        byte[] buffer,
        int offset,
        int count,
        CancellationToken cancellationToken = default)
    {
        Check.Null(buffer);

        return ReadAsync(buffer.AsMemory(offset..count), cancellationToken).AsTask();
    }

    public override sealed int ReadByte()
    {
        var span = (stackalloc byte[1]);

        return Read(span) == 1 ? span[0] : -1;
    }

    public override sealed void Write(byte[] buffer, int offset, int count)
    {
        ValidateBufferArguments(buffer, offset, count);

        Write(buffer.AsSpan(offset..count));
    }

    public override sealed Task WriteAsync(
        byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
    {
        Check.Null(buffer);

        return WriteAsync(buffer.AsMemory(offset..count), cancellationToken).AsTask();
    }

    public override sealed void WriteByte(byte value)
    {
        Write(stackalloc[] { value });
    }
}
