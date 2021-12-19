namespace System.IO;

public abstract class TerminalWriter : TerminalHandle
{
    const int WriteBufferSize = 256;

    public event ReadOnlySpanAction<byte, TerminalWriter>? OutputWritten;

    public TextWriter TextWriter => _writer.Value;

    readonly Lazy<TextWriter> _writer;

    protected TerminalWriter()
    {
        // Unlike TerminalReader, the buffer size here is arbitrary and only has performance implications.
        _writer = new(
            () => TextWriter.Synchronized(new StreamWriter(Stream, Terminal.Encoding, WriteBufferSize, true)
            {
                AutoFlush = true,
            }));
    }

    protected abstract int WriteBufferCore(ReadOnlySpan<byte> buffer, CancellationToken cancellationToken);

    public int WriteBuffer(ReadOnlySpan<byte> buffer, CancellationToken cancellationToken = default)
    {
        var count = WriteBufferCore(buffer, cancellationToken);

        OutputWritten?.Invoke(buffer[..count], this);

        return count;
    }

    public void Write(ReadOnlySpan<byte> value, CancellationToken cancellationToken = default)
    {
        for (var count = 0; count < value.Length; count += WriteBuffer(value[count..], cancellationToken))
        {
        }
    }

    public void Write(ReadOnlySpan<char> value, CancellationToken cancellationToken = default)
    {
        var len = Terminal.Encoding.GetByteCount(value);
        var array = ArrayPool<byte>.Shared.Rent(len);

        try
        {
            var span = array.AsSpan(0, len);

            _ = Terminal.Encoding.GetBytes(value, span);

            Write(span, cancellationToken);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(array);
        }
    }

    public void Write<T>(T value, CancellationToken cancellationToken = default)
    {
        Write((value?.ToString()).AsSpan(), cancellationToken);
    }

    public void WriteLine(CancellationToken cancellationToken = default)
    {
        WriteLine(string.Empty, cancellationToken);
    }

    public void WriteLine<T>(T value, CancellationToken cancellationToken = default)
    {
        Write(value?.ToString() + Environment.NewLine, cancellationToken);
    }
}
