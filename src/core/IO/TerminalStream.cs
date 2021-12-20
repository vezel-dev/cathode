namespace System.IO;

[SuppressMessage("Performance", "CA1844")]
public abstract class TerminalStream : Stream
{
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

    public override sealed void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override sealed long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    public override sealed int Read(byte[] buffer, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        return Read(buffer.AsSpan(offset, count));
    }

    public override sealed Task<int> ReadAsync(
        byte[] buffer,
        int offset,
        int count,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        return ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
    }

    public override sealed unsafe int ReadByte()
    {
        byte value;

        return Read(new Span<byte>(&value, 1)) == 1 ? value : -1;
    }

    public override sealed void Write(byte[] buffer, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        _ = offset >= 0 ? true : throw new ArgumentOutOfRangeException(nameof(offset));
        _ = count >= 0 ? true : throw new ArgumentOutOfRangeException(nameof(count));
        _ = offset + count <= buffer.Length ? true : throw new ArgumentException(null, nameof(buffer));

        Write(buffer.AsSpan(offset, count));
    }

    public override sealed Task WriteAsync(
        byte[] buffer,
        int offset,
        int count,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        var mem = buffer.AsMemory(offset, count);

        static async Task WriteAsyncCore(
            TerminalStream stream,
            Memory<byte> buffer,
            CancellationToken cancellationToken)
        {
            await stream.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
        }

        return WriteAsyncCore(this, mem, cancellationToken);
    }

    public override sealed void WriteByte(byte value)
    {
        Write(stackalloc byte[]
        {
            value,
        });
    }

    public override sealed void Flush()
    {
    }
}
