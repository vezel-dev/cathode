namespace System.IO;

public abstract class TerminalWriter : TerminalHandle
{
    // Unlike TerminalReader, the buffer size here is arbitrary and only has performance implications.
    const int WriteBufferSize = 256;

    public event ReadOnlySpanAction<byte, TerminalWriter>? OutputWritten;

    public TextWriter TextWriter => _writer.Value;

    readonly Lazy<TextWriter> _writer;

    protected TerminalWriter()
    {
        _writer = new(
            () => TextWriter.Synchronized(new StreamWriter(Stream, Terminal.Encoding, WriteBufferSize, true)
            {
                AutoFlush = true,
            }));
    }

    protected abstract int WriteBufferCore(ReadOnlySpan<byte> buffer, CancellationToken cancellationToken);

    protected abstract ValueTask<int> WriteBufferCoreAsync(
        ReadOnlyMemory<byte> buffer,
        CancellationToken cancellationToken);

    public int WriteBuffer(ReadOnlySpan<byte> buffer, CancellationToken cancellationToken = default)
    {
        var count = WriteBufferCore(buffer, cancellationToken);

        OutputWritten?.Invoke(buffer[..count], this);

        return count;
    }

    public async ValueTask<int> WriteBufferAsync(
        ReadOnlyMemory<byte> buffer,
        CancellationToken cancellationToken = default)
    {
        var count = await WriteBufferCoreAsync(buffer, cancellationToken).ConfigureAwait(false);

        OutputWritten?.Invoke(buffer.Span[..count], this);

        return count;
    }

    public void Write(ReadOnlySpan<byte> value, CancellationToken cancellationToken = default)
    {
        for (var count = 0; count < value.Length; count += WriteBuffer(value[count..], cancellationToken))
        {
        }
    }

    public async ValueTask WriteAsync(ReadOnlyMemory<byte> value, CancellationToken cancellationToken = default)
    {
        for (var count = 0;
            count < value.Length;
            count += await WriteBufferAsync(value[count..], cancellationToken).ConfigureAwait(false))
        {
        }
    }

    public ValueTask WriteAsync(Memory<byte> value, CancellationToken cancellationToken = default)
    {
        return WriteAsync((ReadOnlyMemory<byte>)value, cancellationToken);
    }

    public void Write(ReadOnlySpan<char> value, CancellationToken cancellationToken = default)
    {
        var encoding = Terminal.Encoding;
        var len = encoding.GetByteCount(value);
        var array = ArrayPool<byte>.Shared.Rent(len);

        try
        {
            var span = array.AsSpan(0, len);

            _ = encoding.GetBytes(value, span);

            Write(span, cancellationToken);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(array);
        }
    }

    public async ValueTask WriteAsync(ReadOnlyMemory<char> value, CancellationToken cancellationToken = default)
    {
        var encoding = Terminal.Encoding;
        var len = encoding.GetByteCount(value.Span);
        var array = ArrayPool<byte>.Shared.Rent(len);

        try
        {
            var mem = array.AsMemory(0, len);

            _ = encoding.GetBytes(value.Span, mem.Span);

            await WriteAsync(mem, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(array);
        }
    }

    public ValueTask WriteAsync(Memory<char> value, CancellationToken cancellationToken = default)
    {
        return WriteAsync((ReadOnlyMemory<char>)value, cancellationToken);
    }

    public void Write<T>(T value, CancellationToken cancellationToken = default)
    {
        Write((value?.ToString()).AsSpan(), cancellationToken);
    }

    public ValueTask WriteAsync<T>(T value, CancellationToken cancellationToken = default)
    {
        return WriteAsync((value?.ToString()).AsMemory(), cancellationToken);
    }

    public void WriteLine(CancellationToken cancellationToken = default)
    {
        WriteLine(string.Empty, cancellationToken);
    }

    public ValueTask WriteLineAsync(CancellationToken cancellationToken = default)
    {
        return WriteLineAsync(string.Empty, cancellationToken);
    }

    public void WriteLine<T>(T value, CancellationToken cancellationToken = default)
    {
        Write(value?.ToString() + Environment.NewLine, cancellationToken);
    }

    public ValueTask WriteLineAsync<T>(T value, CancellationToken cancellationToken = default)
    {
        return WriteAsync(value?.ToString() + Environment.NewLine, cancellationToken);
    }
}
