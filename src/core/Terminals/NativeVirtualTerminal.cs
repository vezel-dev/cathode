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
            return new(this, descriptor, semaphore);
        }

        NativeTerminalWriter CreateWriter(TerminalInterop.TerminalDescriptor* descriptor, SemaphoreSlim semaphore)
        {
            return new(this, descriptor, semaphore);
        }

        StandardIn = CreateReader(stdIn, inLock);
        StandardOut = CreateWriter(stdOut, outLock);
        StandardError = CreateWriter(stdErr, outLock);
        TerminalIn = CreateReader(ttyIn, inLock);
        TerminalOut = CreateWriter(ttyOut, outLock);
    }

    internal abstract unsafe IDisposable? ArrangeCancellation(
        TerminalInterop.TerminalDescriptor* descriptor, bool write, CancellationToken cancellationToken);

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
