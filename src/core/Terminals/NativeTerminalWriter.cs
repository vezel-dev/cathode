namespace System.Terminals;

abstract class NativeTerminalWriter<TTerminal, THandle> : TerminalWriter
    where TTerminal : NativeVirtualTerminal<THandle>
{
    public TTerminal Terminal { get; }

    public string Name { get; }

    public THandle Handle { get; }

    public override sealed TerminalOutputStream Stream { get; }

    public override sealed bool IsValid { get; }

    public override sealed bool IsInteractive { get; }

    protected NativeTerminalWriter(TTerminal terminal, string name, THandle handle)
    {
        Terminal = terminal;
        Name = name;
        Handle = handle;
        Stream = new(this);
        IsValid = terminal.IsHandleValid(handle, true);
        IsInteractive = terminal.IsHandleInteractive(handle);
    }

    protected override sealed ValueTask<int> WriteBufferCoreAsync(
        ReadOnlyMemory<byte> buffer,
        CancellationToken cancellationToken)
    {
        // We currently have no async support.
        return cancellationToken.IsCancellationRequested ?
            ValueTask.FromCanceled<int>(cancellationToken) :
            new(Task.Run(() => WriteBufferCore(buffer.Span, cancellationToken), cancellationToken));
    }
}
