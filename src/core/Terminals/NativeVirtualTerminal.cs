using Vezel.Cathode.Native;

namespace Vezel.Cathode.Terminals;

internal abstract class NativeVirtualTerminal : SystemVirtualTerminal
{
    public override sealed NativeTerminalReader StandardIn { get; }

    public override sealed NativeTerminalWriter StandardOut { get; }

    public override sealed NativeTerminalWriter StandardError { get; }

    public override sealed NativeTerminalReader TerminalIn { get; }

    public override sealed NativeTerminalWriter TerminalOut { get; }

    [SuppressMessage("", "CA2000")]
    [SuppressMessage("", "CA2214")]
    private protected unsafe NativeVirtualTerminal()
    {
        // Ensure that the native library is fully loaded and initialized before we do anything terminal-related.
        TerminalInterop.Initialize();

        var inLock = new SemaphoreSlim(1, 1);
        var outLock = new SemaphoreSlim(1, 1);

        nuint stdIn;
        nuint stdOut;
        nuint stdErr;
        nuint ttyIn;
        nuint ttyOut;

        TerminalInterop.GetHandles(&stdIn, &stdOut, &stdErr, &ttyIn, &ttyOut);

        StandardIn = CreateReader(stdIn, inLock);
        StandardOut = CreateWriter(stdOut, outLock);
        StandardError = CreateWriter(stdErr, outLock);
        TerminalIn = CreateReader(ttyIn, inLock);
        TerminalOut = CreateWriter(ttyOut, outLock);
    }

    protected abstract NativeTerminalReader CreateReader(nuint handle, SemaphoreSlim semaphore);

    protected abstract NativeTerminalWriter CreateWriter(nuint handle, SemaphoreSlim semaphore);

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
        using var guard = Control.Guard();

        TerminalInterop.GenerateSignal(signal).ThrowIfError(signal);
    }
}
