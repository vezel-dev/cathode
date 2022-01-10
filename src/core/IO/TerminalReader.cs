namespace System.IO;

public abstract class TerminalReader : TerminalHandle
{
    public event SpanAction<byte, TerminalReader>? InputRead;

    public abstract TextReader TextReader { get; }

    protected abstract int ReadPartialCore(Span<byte> buffer, CancellationToken cancellationToken);

    protected abstract ValueTask<int> ReadPartialCoreAsync(Memory<byte> buffer, CancellationToken cancellationToken);

    public int ReadPartial(Span<byte> buffer, CancellationToken cancellationToken = default)
    {
        var count = ReadPartialCore(buffer, cancellationToken);

        InputRead?.Invoke(buffer[..count], this);

        return count;
    }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
    public async ValueTask<int> ReadPartialAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var count = await ReadPartialCoreAsync(buffer, cancellationToken).ConfigureAwait(false);

        InputRead?.Invoke(buffer.Span[..count], this);

        return count;
    }

    public int Read(Span<byte> value, CancellationToken cancellationToken = default)
    {
        var count = 0;

        while (count < value.Length)
        {
            var ret = ReadPartial(value[count..], cancellationToken);

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
            var ret = await ReadPartialAsync(value[count..], cancellationToken).ConfigureAwait(false);

            // EOF?
            if (ret == 0)
                break;

            count += ret;
        }

        return count;
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
