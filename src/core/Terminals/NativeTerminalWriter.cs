using Vezel.Cathode.Native;

namespace Vezel.Cathode.Terminals;

internal abstract class NativeTerminalWriter : TerminalWriter
{
    // Unlike NativeTerminalReader, the buffer size here is arbitrary and only has performance implications.
    private const int WriteBufferSize = 256;

    public NativeVirtualTerminal Terminal { get; }

    public nuint Handle { get; }

    public override sealed Stream Stream { get; }

    public override sealed TextWriter TextWriter { get; }

    public override sealed bool IsValid { get; }

    public override sealed bool IsInteractive { get; }

    protected NativeTerminalWriter(NativeVirtualTerminal terminal, nuint handle)
    {
        Terminal = terminal;
        Handle = handle;
        Stream = new SynchronizedStream(new TerminalOutputStream(this));
        TextWriter =
            new SynchronizedTextWriter(new StreamWriter(Stream, Cathode.Terminal.Encoding, WriteBufferSize, true)
            {
                AutoFlush = true,
            });
        IsValid = TerminalInterop.IsValid(handle, write: true);
        IsInteractive = TerminalInterop.IsInteractive(handle);
    }

    protected abstract int WritePartialNative(scoped ReadOnlySpan<byte> buffer, CancellationToken cancellationToken);

    protected override sealed int WritePartialCore(scoped ReadOnlySpan<byte> buffer)
    {
        return WritePartialNative(buffer, default);
    }

    protected override sealed ValueTask<int> WritePartialCoreAsync(
        ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
    {
        // We currently have no native async support.
        return cancellationToken.IsCancellationRequested
            ? ValueTask.FromCanceled<int>(cancellationToken)
            : new(Task.Run(() => WritePartialNative(buffer.Span, cancellationToken), cancellationToken));
    }
}
