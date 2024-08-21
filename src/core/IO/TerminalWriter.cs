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
}
