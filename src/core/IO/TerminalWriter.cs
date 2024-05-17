// SPDX-License-Identifier: 0BSD

namespace Vezel.Cathode.IO;

public abstract class TerminalWriter : TerminalHandle
{
    public event ReadOnlySpanAction<byte, TerminalWriter>? OutputWritten;

    public abstract TextWriter TextWriter { get; }

    protected abstract int WritePartialCore(scoped ReadOnlySpan<byte> buffer);

    protected abstract ValueTask<int> WritePartialCoreAsync(
        ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken);

    public int WritePartial(scoped ReadOnlySpan<byte> buffer)
    {
        var count = WritePartialCore(buffer);

        OutputWritten?.Invoke(buffer[..count], this);

        return count;
    }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
    public async ValueTask<int> WritePartialAsync(
        ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var count = await WritePartialCoreAsync(buffer, cancellationToken).ConfigureAwait(false);

        OutputWritten?.Invoke(buffer.Span[..count], this);

        return count;
    }

    public void Write(scoped ReadOnlySpan<byte> value)
    {
        for (var count = 0; count < value.Length; count += WritePartial(value[count..]))
        {
        }
    }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder))]
    public async ValueTask WriteAsync(ReadOnlyMemory<byte> value, CancellationToken cancellationToken = default)
    {
        for (var count = 0;
            count < value.Length;
            count += await WritePartialAsync(value[count..], cancellationToken).ConfigureAwait(false))
        {
        }
    }

    public ValueTask WriteAsync(Memory<byte> value, CancellationToken cancellationToken = default)
    {
        return WriteAsync((ReadOnlyMemory<byte>)value, cancellationToken);
    }

    public ValueTask WriteAsync(byte[]? value, CancellationToken cancellationToken = default)
    {
        return WriteAsync(value.AsMemory(), cancellationToken);
    }

    public void Write(scoped ReadOnlySpan<char> value)
    {
        var encoding = Terminal.Encoding;
        var len = encoding.GetByteCount(value);
        var array = ArrayPool<byte>.Shared.Rent(len);

        try
        {
            var span = array.AsSpan(..len);

            _ = encoding.GetBytes(value, span);

            Write(span);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(array);
        }
    }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder))]
    public async ValueTask WriteAsync(ReadOnlyMemory<char> value, CancellationToken cancellationToken = default)
    {
        var encoding = Terminal.Encoding;
        var len = encoding.GetByteCount(value.Span);
        var array = ArrayPool<byte>.Shared.Rent(len);

        try
        {
            var mem = array.AsMemory(..len);

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

    public ValueTask WriteAsync(char[]? value, CancellationToken cancellationToken = default)
    {
        return WriteAsync(value.AsMemory(), cancellationToken);
    }

    public ValueTask WriteAsync(string? value, CancellationToken cancellationToken = default)
    {
        return WriteAsync(value.AsMemory(), cancellationToken);
    }

    public void Write<T>(T value)
    {
        Write((value?.ToString()).AsSpan());
    }

    public ValueTask WriteAsync<T>(T value, CancellationToken cancellationToken = default)
    {
        return WriteAsync((value?.ToString()).AsMemory(), cancellationToken);
    }

    public void WriteLine()
    {
        WriteLine(string.Empty);
    }

    public ValueTask WriteLineAsync(CancellationToken cancellationToken = default)
    {
        return WriteLineAsync(string.Empty, cancellationToken);
    }

    public void WriteLine<T>(T value)
    {
        Write(value?.ToString() + Environment.NewLine);
    }

    public ValueTask WriteLineAsync<T>(T value, CancellationToken cancellationToken = default)
    {
        return WriteAsync(value?.ToString() + Environment.NewLine, cancellationToken);
    }
}
