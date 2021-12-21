namespace System.Terminals;

abstract class NativeTerminalReader<TTerminal, THandle> : TerminalReader
    where TTerminal : NativeVirtualTerminal<THandle>
{
    public TTerminal Terminal { get; }

    public string Name { get; }

    public THandle Handle { get; }

    public override sealed TerminalInputStream Stream { get; }

    public override sealed bool IsValid { get; }

    public override sealed bool IsInteractive { get; }

    protected NativeTerminalReader(TTerminal terminal, string name, THandle handle)
    {
        Terminal = terminal;
        Name = name;
        Handle = handle;
        Stream = new(this);
        IsValid = terminal.IsHandleValid(handle, false);
        IsInteractive = terminal.IsHandleInteractive(handle);
    }

    protected override sealed ValueTask<int> ReadBufferCoreAsync(
        Memory<byte> buffer,
        CancellationToken cancellationToken)
    {
        // We currently have no async support.
        return cancellationToken.IsCancellationRequested ?
            ValueTask.FromCanceled<int>(cancellationToken) :
            new(Task.Run(() => ReadBufferCore(buffer.Span, cancellationToken), cancellationToken));
    }
}
