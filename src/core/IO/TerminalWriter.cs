namespace System.IO;

public abstract class TerminalWriter : TerminalHandle
{
    const int WriteBufferSize = 256;

    public event ReadOnlySpanAction<byte, TerminalWriter>? OutputWritten;

    public TextWriter Writer => _writer.Value;

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

    protected abstract void WriteCore(ReadOnlySpan<byte> data, out int count);

    public void Write(ReadOnlySpan<byte> data, out int count)
    {
        count = 0;

        // WriteCore is required to assign count appropriately for partial writes that fail.
        try
        {
            WriteCore(data, out count);
        }
        finally
        {
            OutputWritten?.Invoke(data[..count], this);
        }
    }

    public void Write(ReadOnlySpan<byte> data)
    {
        Write(data, out _);
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
