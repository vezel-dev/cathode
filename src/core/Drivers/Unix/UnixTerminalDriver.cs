using static System.Unix.UnixConstants;
using static System.Unix.UnixPInvoke;

namespace System.Drivers.Unix;

abstract class UnixTerminalDriver : TerminalDriver<int>
{
    public override sealed UnixTerminalReader StandardIn { get; }

    public override sealed UnixTerminalWriter StandardOut { get; }

    public override sealed UnixTerminalWriter StandardError { get; }

    public override sealed UnixTerminalReader TerminalIn { get; }

    public override sealed UnixTerminalWriter TerminalOut { get; }

    [SuppressMessage("Style", "IDE0052")]
    readonly PosixSignalRegistration _sigWinch;

    [SuppressMessage("Style", "IDE0052")]
    readonly PosixSignalRegistration _sigCont;

    readonly object _rawLock = new();

    [SuppressMessage("Usage", "CA2214")]
    protected UnixTerminalDriver()
    {
        var inLock = new object();
        var outLock = new object();

        StandardIn = new(this, "standard input", STDIN_FILENO, inLock);
        StandardOut = new(this, "standard output", STDOUT_FILENO, outLock);
        StandardError = new(this, "standard error", STDERR_FILENO, new());

        var tty = OpenTerminalHandle("/dev/tty");

        TerminalIn = new(this, "terminal input", tty, inLock);
        TerminalOut = new(this, "terminal output", tty, outLock);

        RefreshSize();

        void HandleSignal(PosixSignalContext context)
        {
            // If we are being restored from the background (SIGCONT), it is possible and likely that terminal settings
            // have been mangled, so restore them.
            if (context.Signal == PosixSignal.SIGCONT)
            {
                lock (_rawLock)
                    RefreshSettings();

                // Prevent System.Native from overwriting the terminal settings we just put into effect.
                context.Cancel = true;
            }

            // Terminal width/height might have changed for SIGCONT, and will definitely have changed for SIGWINCH. On
            // Unix systems, SIGWINCH lets us respond much more quickly to a change in terminal size.
            RefreshSize();
        }

        // Keep the registrations alive by storing them in fields.
        _sigWinch = PosixSignalRegistration.Create(PosixSignal.SIGWINCH, HandleSignal);
        _sigCont = PosixSignalRegistration.Create(PosixSignal.SIGCONT, HandleSignal);

        // TODO: SIGCHLD?
    }

    public override sealed void GenerateSignal(TerminalSignal signal)
    {
        _ = kill(
            0,
            signal switch
            {
                TerminalSignal.Close => SIGHUP,
                TerminalSignal.Interrupt => SIGINT,
                TerminalSignal.Quit => SIGQUIT,
                TerminalSignal.Terminate => SIGTERM,
                _ => throw new ArgumentOutOfRangeException(nameof(signal)),
            });
    }

    protected abstract void RefreshSettings();

    protected override sealed void SetRawMode(bool raw)
    {
        lock (_rawLock)
            if (!SetRawModeCore(raw))
                throw new TerminalException("There is no terminal attached.");
    }

    protected abstract bool SetRawModeCore(bool raw);

    public abstract int OpenTerminalHandle(string name);

    public abstract bool PollHandle(int error, int handle, short events);

    public override bool IsHandleValid(int handle, bool write)
    {
        // We might obtain a negative descriptor (-1) if we fail to open /dev/tty, for example.
        return handle >= 0;
    }

    public override bool IsHandleInteractive(int handle)
    {
        // Note that this also returns false for invalid descriptors.
        return isatty(handle) == 1;
    }
}
