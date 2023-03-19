using Vezel.Cathode.Threading;

namespace Vezel.Cathode.IO;

[SuppressMessage("", "CA2213")]
internal sealed class SynchronizedTextReader : TextReader
{
    // The reader returned by TextReader.Synchronized has no async support and also lacks forwarding for newer method
    // overloads. This class addresses those issues.
    //
    // Note that Close, Dispose, and DisposeAsync are intentionally not forwarded as this class is meant to wrap readers
    // that are disposed by other means or are intended to live for the duration of the program.

    private readonly SemaphoreSlim _lock = new(1, 1);

    private readonly TextReader _reader;

    public SynchronizedTextReader(TextReader reader)
    {
        _reader = reader;
    }

    public override int Peek()
    {
        using (_lock.Enter())
            return _reader.Peek();
    }

    public override int Read()
    {
        using (_lock.Enter())
            return _reader.Read();
    }

    public override int Read(char[] buffer, int index, int count)
    {
        using (_lock.Enter())
            return _reader.Read(buffer, index, count);
    }

    public override int Read(Span<char> buffer)
    {
        using (_lock.Enter())
            return _reader.Read(buffer);
    }

    public override async Task<int> ReadAsync(char[] buffer, int index, int count)
    {
        using (await _lock.EnterAsync().ConfigureAwait(false))
            return await _reader.ReadAsync(buffer, index, count).ConfigureAwait(false);
    }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
    public override async ValueTask<int> ReadAsync(Memory<char> buffer, CancellationToken cancellationToken = default)
    {
        using (await _lock.EnterAsync(cancellationToken).ConfigureAwait(false))
            return await _reader.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
    }

    public override string ReadToEnd()
    {
        using (_lock.Enter())
            return _reader.ReadToEnd();
    }

    public override async Task<string> ReadToEndAsync()
    {
        using (await _lock.EnterAsync().ConfigureAwait(false))
            return await _reader.ReadToEndAsync().ConfigureAwait(false);
    }

    public override int ReadBlock(char[] buffer, int index, int count)
    {
        using (_lock.Enter())
            return _reader.ReadBlock(buffer, index, count);
    }

    public override int ReadBlock(Span<char> buffer)
    {
        using (_lock.Enter())
            return _reader.ReadBlock(buffer);
    }

    public override async Task<int> ReadBlockAsync(char[] buffer, int index, int count)
    {
        using (await _lock.EnterAsync().ConfigureAwait(false))
            return await _reader.ReadBlockAsync(buffer, index, count).ConfigureAwait(false);
    }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
    public override async ValueTask<int> ReadBlockAsync(
        Memory<char> buffer, CancellationToken cancellationToken = default)
    {
        using (await _lock.EnterAsync(cancellationToken).ConfigureAwait(false))
            return await _reader.ReadBlockAsync(buffer, cancellationToken).ConfigureAwait(false);
    }

    public override string? ReadLine()
    {
        using (_lock.Enter())
            return _reader.ReadLine();
    }

    public override async Task<string?> ReadLineAsync()
    {
        using (await _lock.EnterAsync().ConfigureAwait(false))
            return await _reader.ReadLineAsync().ConfigureAwait(false);
    }
}
