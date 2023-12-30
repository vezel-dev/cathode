using Vezel.Cathode.Native;

namespace Vezel.Cathode.Terminals;

internal abstract class NativeTerminalReader : TerminalReader
{
    // Note that the buffer size used affects how many characters the Windows console host will allow the user to type
    // in a single line (in cooked mode).
    private const int ReadBufferSize = 4096;

    public NativeVirtualTerminal Terminal { get; }

    public nuint Handle { get; }

    public override sealed Stream Stream { get; }

    public override sealed TextReader TextReader { get; }

    public override sealed bool IsValid { get; }

    public override sealed bool IsInteractive { get; }

    protected NativeTerminalReader(NativeVirtualTerminal terminal, nuint handle)
    {
        Terminal = terminal;
        Handle = handle;
        Stream = new SynchronizedStream(new TerminalInputStream(this));
        TextReader =
            new SynchronizedTextReader(
                new StreamReader(Stream, Cathode.Terminal.Encoding, false, ReadBufferSize, true));
        IsValid = TerminalInterop.IsValid(handle, write: false);
        IsInteractive = TerminalInterop.IsInteractive(handle);
    }

    protected abstract int ReadPartialNative(scoped Span<byte> buffer, CancellationToken cancellationToken);

    protected override sealed int ReadPartialCore(scoped Span<byte> buffer)
    {
        return ReadPartialNative(buffer, default);
    }

    protected override sealed ValueTask<int> ReadPartialCoreAsync(
        Memory<byte> buffer, CancellationToken cancellationToken)
    {
        // We currently have no native async support.
        return cancellationToken.IsCancellationRequested
            ? ValueTask.FromCanceled<int>(cancellationToken)
            : new(Task.Run(() => ReadPartialNative(buffer.Span, cancellationToken), cancellationToken));
    }
}
