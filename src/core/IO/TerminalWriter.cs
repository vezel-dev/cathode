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

    public void Write(ReadOnlySpan<char> value)
    {
        var len = Terminal.Encoding.GetByteCount(value);
        var array = ArrayPool<byte>.Shared.Rent(len);

        try
        {
            var span = array.AsSpan(0, len);

            _ = Terminal.Encoding.GetBytes(value, span);

            Write(span);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(array);
        }
    }

    public void Write(string? value)
    {
        Write(value.AsSpan());
    }

    public void Write<T>(T value)
    {
        Write(value?.ToString());
    }

    public void Write(string format, params object?[] args)
    {
        Write(string.Format(CultureInfo.CurrentCulture, format, args));
    }

    public void WriteLine()
    {
        WriteLine(null);
    }

    public void WriteLine(string? value)
    {
        Write(value + Environment.NewLine);
    }

    public void WriteLine<T>(T value)
    {
        WriteLine(value?.ToString());
    }

    public void WriteLine(string format, params object?[] args)
    {
        WriteLine(string.Format(CultureInfo.CurrentCulture, format, args));
    }
}
