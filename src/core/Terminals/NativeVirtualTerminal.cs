using Vezel.Cathode.Native;

namespace Vezel.Cathode.Terminals;

internal abstract class NativeVirtualTerminal : SystemVirtualTerminal
{
    public override sealed NativeTerminalReader StandardIn { get; }

    public override sealed NativeTerminalWriter StandardOut { get; }

    public override sealed NativeTerminalWriter StandardError { get; }

    public override sealed NativeTerminalReader TerminalIn { get; }

    public override sealed NativeTerminalWriter TerminalOut { get; }

    private protected unsafe NativeVirtualTerminal()
    {
        // Ensure that the native library is fully loaded and initialized before we do anything terminal-related.
        TerminalInterop.Initialize();

        var inLock = new SemaphoreSlim(1, 1);
        var outLock = new SemaphoreSlim(1, 1);

        TerminalInterop.TerminalDescriptor* stdIn;
        TerminalInterop.TerminalDescriptor* stdOut;
        TerminalInterop.TerminalDescriptor* stdErr;
        TerminalInterop.TerminalDescriptor* ttyIn;
        TerminalInterop.TerminalDescriptor* ttyOut;

        TerminalInterop.GetDescriptors(&stdIn, &stdOut, &stdErr, &ttyIn, &ttyOut);

        NativeTerminalReader CreateReader(TerminalInterop.TerminalDescriptor* descriptor, SemaphoreSlim semaphore)
        {
            return new(this, descriptor, semaphore, CreateCancellationHook(write: false));
        }

        NativeTerminalWriter CreateWriter(TerminalInterop.TerminalDescriptor* descriptor, SemaphoreSlim semaphore)
        {
            return new(this, descriptor, semaphore, CreateCancellationHook(write: true));
        }

        StandardIn = CreateReader(stdIn, inLock);
        StandardOut = CreateWriter(stdOut, outLock);
        StandardError = CreateWriter(stdErr, outLock);
        TerminalIn = CreateReader(ttyIn, inLock);
        TerminalOut = CreateWriter(ttyOut, outLock);
    }

    protected abstract Action<nuint, CancellationToken>? CreateCancellationHook(bool write);

    private protected override sealed unsafe Size? QuerySize()
    {
        int width;
        int height;

        return TerminalInterop.QuerySize(&width, &height) ? new(width, height) : null;
    }

    private protected override sealed bool GetMode()
    {
        return TerminalInterop.GetMode();
    }

    private protected override sealed void SetMode(bool raw, bool flush)
    {
        TerminalInterop.SetMode(raw, flush).ThrowIfError();
    }

    public override sealed void GenerateSignal(TerminalSignal signal)
    {
        using (Control.Guard())
            TerminalInterop.GenerateSignal(signal).ThrowIfError(signal);
    }
}
