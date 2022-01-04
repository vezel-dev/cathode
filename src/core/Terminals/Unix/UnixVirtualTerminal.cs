using static System.Unix.UnixPInvoke;

namespace System.Terminals.Unix;

abstract class UnixVirtualTerminal : NativeVirtualTerminal<int>
{
    public override event Action? Resumed;

    public override sealed UnixTerminalReader StandardIn { get; }

    public override sealed UnixTerminalWriter StandardOut { get; }

    public override sealed UnixTerminalWriter StandardError { get; }

    public override sealed UnixTerminalReader TerminalIn { get; }

    public override sealed UnixTerminalWriter TerminalOut { get; }

    [SuppressMessage("Style", "IDE0052")]
    readonly PosixSignalRegistration _sigWinch;

    [SuppressMessage("Style", "IDE0052")]
    readonly PosixSignalRegistration _sigCont;

    [SuppressMessage("Style", "IDE0052")]
    readonly PosixSignalRegistration _sigChld;

    readonly object _rawLock = new();

    [SuppressMessage("Usage", "CA2214")]
    protected UnixVirtualTerminal()
    {
        var inLock = new object();
        var outLock = new object();

        StandardIn = new(this, "standard input", STDIN_FILENO, new(this), inLock);
        StandardOut = new(this, "standard output", STDOUT_FILENO, outLock);
        StandardError = new(this, "standard error", STDERR_FILENO, outLock);

        var tty = OpenTerminalHandle("/dev/tty");

        TerminalIn = new(this, "terminal input", tty, new(this), inLock);
        TerminalOut = new(this, "terminal output", tty, outLock);

        try
        {
            // Start in cooked mode.
            SetRawModeCore(false, false);
        }
        catch (Exception e) when (e is TerminalNotAttachedException or TerminalConfigurationException)
        {
        }

        RefreshSize();

        void HandleSignal(PosixSignalContext context)
        {
            // If we are being restored from the background (SIGCONT), it is possible and likely that terminal settings
            // have been mangled, so restore them.
            //
            // This is a best-effort thing. The reality is that, since this signal handler method gets called in a
            // thread after the process has fully woken up, other code may already be trying to interact with the
            // terminal again. There is currently nothing we can really do about this race condition.
            if (context.Signal == PosixSignal.SIGCONT)
            {
                try
                {
                    lock (_rawLock)
                        SetRawModeCore(IsRawMode, false);
                }
                catch (Exception e) when (e is TerminalNotAttachedException or TerminalConfigurationException)
                {
                    // Either there was no terminal attached to begin with, or it has disappeared since we were stopped.
                    // In either case, the program can no longer read from or write to the terminal, so terminal
                    // settings are irrelevant.
                }

                // Do this on the thread pool to avoid breaking internals if an event handler misbehaves.
                _ = ThreadPool.UnsafeQueueUserWorkItem(state => Resumed?.Invoke(), null);
            }

            // Terminal width/height will definitely have changed for SIGWINCH, and might have changed for SIGCONT. On
            // Unix systems, SIGWINCH lets us respond much more quickly to a change in terminal size.
            if (context.Signal is PosixSignal.SIGWINCH or PosixSignal.SIGCONT)
                RefreshSize();

            // Prevent System.Native from overwriting our terminal settings.
            context.Cancel = true;
        }

        // Keep the registrations alive by storing them in fields.
        _sigWinch = PosixSignalRegistration.Create(PosixSignal.SIGWINCH, HandleSignal);
        _sigCont = PosixSignalRegistration.Create(PosixSignal.SIGCONT, HandleSignal);
        _sigChld = PosixSignalRegistration.Create(PosixSignal.SIGCHLD, HandleSignal);
    }

    protected override sealed void SendSignal(int pid, TerminalSignal signal)
    {
        _ = kill(
            pid,
            signal switch
            {
                TerminalSignal.Close => SIGHUP,
                TerminalSignal.Interrupt => SIGINT,
                TerminalSignal.Quit => SIGQUIT,
                TerminalSignal.Terminate => SIGTERM,
                _ => throw new ArgumentOutOfRangeException(nameof(signal)),
            });
    }

    protected abstract void SetRawModeCore(bool raw, bool flush);

    protected override sealed void SetRawMode(bool raw)
    {
        // We can be called from signal handlers so we need an additional lock here.
        lock (_rawLock)
            SetRawModeCore(raw, true);
    }

    public abstract int OpenTerminalHandle(string name);

    public abstract bool PollHandles(int? error, short events, Span<int> handles);

    public override sealed bool IsHandleValid(int handle, bool write)
    {
        // We might obtain a negative descriptor (-1) if we fail to open /dev/tty, for example.
        return handle >= 0;
    }

    public override sealed bool IsHandleInteractive(int handle)
    {
        // Note that this also returns false for invalid descriptors.
        return isatty(handle) == 1;
    }
}
