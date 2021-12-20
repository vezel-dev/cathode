namespace System.Drivers;

abstract class DriverTerminalWriter<TDriver, THandle> : TerminalWriter
    where TDriver : TerminalDriver<THandle>
{
    public TDriver Driver { get; }

    public string Name { get; }

    public THandle Handle { get; }

    public override sealed TerminalOutputStream Stream { get; }

    public override sealed bool IsValid { get; }

    public override sealed bool IsInteractive { get; }

    protected DriverTerminalWriter(TDriver driver, string name, THandle handle)
    {
        Driver = driver;
        Name = name;
        Handle = handle;
        Stream = new(this);
        IsValid = driver.IsHandleValid(handle, true);
        IsInteractive = driver.IsHandleInteractive(handle);
    }

    protected override sealed ValueTask<int> WriteBufferCoreAsync(
        ReadOnlyMemory<byte> buffer,
        CancellationToken cancellationToken)
    {
        // We currently have no async support in the drivers.
        return cancellationToken.IsCancellationRequested ?
            ValueTask.FromCanceled<int>(cancellationToken) :
            new(Task.Run(() => WriteBufferCore(buffer.Span, cancellationToken), cancellationToken));
    }
}
