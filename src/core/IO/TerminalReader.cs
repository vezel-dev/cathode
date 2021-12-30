namespace System.IO;

public abstract class TerminalReader : TerminalHandle
{
    // Note that the buffer size used affects how many characters the Windows console host will allow the user to type
    // in a single line (in cooked mode).
    const int ReadBufferSize = 4096;

    public event SpanAction<byte, TerminalReader>? InputRead;

    public TextReader TextReader => _reader.Value;

    readonly Lazy<TextReader> _reader;

    protected TerminalReader()
    {
        _reader = new(
            () => TextReader.Synchronized(new StreamReader(Stream, Terminal.Encoding, false, ReadBufferSize, true)));
    }

    protected abstract int ReadBufferCore(Span<byte> buffer, CancellationToken cancellationToken);

    protected abstract ValueTask<int> ReadBufferCoreAsync(Memory<byte> buffer, CancellationToken cancellationToken);

    public int ReadBuffer(Span<byte> buffer, CancellationToken cancellationToken = default)
    {
        var count = ReadBufferCore(buffer, cancellationToken);

        InputRead?.Invoke(buffer[..count], this);

        return count;
    }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
    public async ValueTask<int> ReadBufferAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var count = await ReadBufferCoreAsync(buffer, cancellationToken).ConfigureAwait(false);

        InputRead?.Invoke(buffer.Span[..count], this);

        return count;
    }

    public int Read(Span<byte> value, CancellationToken cancellationToken = default)
    {
        var count = 0;

        while (count < value.Length)
        {
            var ret = ReadBuffer(value[count..], cancellationToken);

            // EOF?
            if (ret == 0)
                break;

            count += ret;
        }

        return count;
    }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
    public async ValueTask<int> ReadAsync(Memory<byte> value, CancellationToken cancellationToken = default)
    {
        var count = 0;

        while (count < value.Length)
        {
            var ret = await ReadBufferAsync(value[count..], cancellationToken).ConfigureAwait(false);

            // EOF?
            if (ret == 0)
                break;

            count += ret;
        }

        return count;
    }

    public unsafe byte? ReadRaw(CancellationToken cancellationToken = default)
    {
        byte value;

        return Read(new Span<byte>(&value, 1), cancellationToken) == 1 ? value : null;
    }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
    public async ValueTask<byte?> ReadRawAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Can we optimize this?
        var array = ArrayPool<byte>.Shared.Rent(1);

        try
        {
            return await ReadAsync(array.AsMemory(..1), cancellationToken).ConfigureAwait(false) == 1 ?
                MemoryMarshal.GetArrayDataReference(array) : null;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(array);
        }
    }

    public string? ReadLine(CancellationToken cancellationToken = default)
    {
        // TODO: Pass on the cancellation token in .NET 7.
        cancellationToken.ThrowIfCancellationRequested();

        return TextReader.ReadLine();
    }

    public ValueTask<string?> ReadLineAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Pass on the cancellation token in .NET 7.
        return cancellationToken.IsCancellationRequested ?
            ValueTask.FromCanceled<string?>(cancellationToken) :
            new(TextReader.ReadLineAsync());
    }
}
