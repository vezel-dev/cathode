namespace System.IO;

public sealed class TerminalInputStream : Stream
{
    public TerminalReader Reader { get; }

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    internal TerminalInputStream(TerminalReader reader)
    {
        Reader = reader;
    }

    // We intentionally do not implement Dispose.

    public override void Flush()
    {
    }

    public override int Read(Span<byte> buffer)
    {
        return Reader.Read(buffer);
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        _ = offset >= 0 ? true : throw new ArgumentOutOfRangeException(nameof(offset));
        _ = count >= 0 ? true : throw new ArgumentOutOfRangeException(nameof(count));
        _ = offset + count <= buffer.Length ? true : throw new ArgumentException(null, nameof(buffer));

        return Read(buffer.AsSpan(offset, count));
    }

    public override int ReadByte()
    {
        Span<byte> buf = stackalloc byte[1];

        return Read(buf) == buf.Length ? buf[0] : -1;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }
}
