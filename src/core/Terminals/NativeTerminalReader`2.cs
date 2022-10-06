using Vezel.Cathode.IO;

namespace Vezel.Cathode.Terminals;

internal abstract class NativeTerminalReader<TTerminal, THandle> : TerminalReader
    where TTerminal : NativeVirtualTerminal<THandle>
{
    // Note that the buffer size used affects how many characters the Windows console host will allow the user to type
    // in a single line (in cooked mode).
    private const int ReadBufferSize = 4096;

    public TTerminal Terminal { get; }

    public string Name { get; }

    public THandle Handle { get; }

    public override sealed Stream Stream { get; }

    public override sealed TextReader TextReader { get; }

    public override sealed bool IsValid { get; }

    public override sealed bool IsInteractive { get; }

    protected NativeTerminalReader(TTerminal terminal, string name, THandle handle)
    {
        Terminal = terminal;
        Name = name;
        Handle = handle;
        Stream = new SynchronizedStream(new TerminalInputStream(this));
        TextReader =
            new SynchronizedTextReader(
                new StreamReader(Stream, Cathode.Terminal.Encoding, false, ReadBufferSize, true));
        IsValid = terminal.IsHandleValid(handle, false);
        IsInteractive = terminal.IsHandleInteractive(handle);
    }

    protected override sealed ValueTask<int> ReadPartialCoreAsync(
        Memory<byte> buffer,
        CancellationToken cancellationToken)
    {
        // We currently have no async support.
        return cancellationToken.IsCancellationRequested ?
            ValueTask.FromCanceled<int>(cancellationToken) :
            new(Task.Run(() => ReadPartialCore(buffer.Span, cancellationToken), cancellationToken));
    }
}
