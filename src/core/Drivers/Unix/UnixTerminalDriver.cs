using static System.Unix.UnixConstants;
using static System.Unix.UnixPInvoke;

namespace System.Drivers.Unix;

abstract class UnixTerminalDriver : TerminalDriver
{
    public override UnixTerminalReader StdIn { get; }

    public override UnixTerminalWriter StdOut { get; }

    public override UnixTerminalWriter StdError { get; }

    [SuppressMessage("Style", "IDE0052")]
    readonly PosixSignalRegistration _sigWinch;

    [SuppressMessage("Style", "IDE0052")]
    readonly PosixSignalRegistration _sigCont;

    readonly object _rawLock = new();

    protected UnixTerminalDriver()
    {
        StdIn = new(this, STDIN_FILENO);
        StdOut = new(this, STDOUT_FILENO, "output");
        StdError = new(this, STDERR_FILENO, "error");

        RefreshSize();

        void HandleSignal(PosixSignalContext context)
        {
            // If we are being restored from the background (SIGCONT), it is possible and likely that terminal settings
            // have been mangled, so restore them.
            if (context.Signal == PosixSignal.SIGCONT)
                lock (_rawLock)
                    RefreshSettings();

            // Terminal width/height might have changed for SIGCONT, and will definitely have changed for SIGWINCH.
            //
            // We currently trust that SIGWINCH will always be delivered when the terminal size changes. If this ever
            // turns out to be false somewhere/somehow, we may need to use a background thread to also poll for size
            // changes like in the Windows driver.
            RefreshSize();
        }

        // Keep the registrations alive by storing them in fields.
        _sigWinch = PosixSignalRegistration.Create(PosixSignal.SIGWINCH, HandleSignal);
        _sigCont = PosixSignalRegistration.Create(PosixSignal.SIGCONT, HandleSignal);

        // TODO: SIGCHLD?
    }

    public override void GenerateSignal(TerminalSignal signal)
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

    protected override void SetRawMode(bool raw)
    {
        lock (_rawLock)
            if (!SetRawModeCore(raw))
                throw new TerminalException("There is no terminal attached.");
    }

    protected abstract bool SetRawModeCore(bool raw);

    public abstract bool PollHandle(int error, int handle, short events);
}
