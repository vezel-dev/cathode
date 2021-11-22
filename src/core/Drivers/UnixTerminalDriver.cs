using static System.Unix.UnixConstants;
using static System.Unix.UnixPInvoke;

namespace System.Drivers;

abstract class UnixTerminalDriver : TerminalDriver
{
    sealed class UnixTerminalReader : TerminalReader
    {
        public int Handle { get; }

        public override bool IsRedirected => IsRedirected(Handle);

        readonly object _lock = new();

        readonly string _name;

        public UnixTerminalReader(TerminalDriver driver, int handle, string name)
            : base(driver)
        {
            Handle = handle;
            _name = name;
        }

        public override unsafe int Read(Span<byte> data)
        {
            if (data.IsEmpty)
                return 0;

            long ret;

            lock (_lock)
            {
                while (true)
                {
                    fixed (byte* p = data)
                    {
                        while ((ret = read(Handle, p, (nuint)data.Length)) == -1 &&
                            Marshal.GetLastPInvokeError() == EINTR)
                        {
                            // Retry in case we get interrupted by a signal.
                        }

                        if (ret != -1)
                            break;

                        var err = Marshal.GetLastPInvokeError();

                        // The descriptor was probably redirected to a program that ended. Just silently ignore this
                        // situation.
                        //
                        // The strange condition where errno is zero happens e.g. on Linux if the process is killed
                        // while blocking in the read system call.
                        if (err is 0 or EPIPE)
                        {
                            ret = 0;

                            break;
                        }

                        // The file descriptor has been configured as non-blocking. Instead of busily trying to read
                        // over and over, poll until we can write and then try again.
                        if (((UnixTerminalDriver)Driver).PollHandle(err, Handle, POLLIN))
                            continue;

                        throw new TerminalException($"Could not read from standard {_name}: {err}");
                    }
                }
            }

            return (int)ret;
        }
    }

    sealed class UnixTerminalWriter : TerminalWriter
    {
        public int Handle { get; }

        public override bool IsRedirected => IsRedirected(Handle);

        readonly object _lock = new();

        readonly string _name;

        public UnixTerminalWriter(TerminalDriver driver, int handle, string name)
            : base(driver)
        {
            Handle = handle;
            _name = name;
        }

        public override unsafe void Write(ReadOnlySpan<byte> data)
        {
            if (data.IsEmpty)
                return;

            lock (_lock)
            {
                var progress = 0;

                fixed (byte* p = data)
                {
                    var len = data.Length;

                    while (progress < len)
                    {
                        long ret;

                        while ((ret = write(Handle, p + progress, (nuint)(len - progress))) == -1 &&
                            Marshal.GetLastPInvokeError() == EINTR)
                        {
                        }

                        // The descriptor has been closed by someone else. Just silently ignore this situation.
                        if (ret == 0)
                            break;

                        if (ret != -1)
                        {
                            progress += (int)ret;

                            continue;
                        }

                        var err = Marshal.GetLastPInvokeError();

                        // The descriptor was probably redirected to a program that ended. Just silently ignore this
                        // situation.
                        if (err == EPIPE)
                            break;

                        // The file descriptor has been configured as non-blocking. Instead of busily trying to write
                        // over and over, poll until we can write and then try again.
                        if (((UnixTerminalDriver)Driver).PollHandle(err, Handle, POLLOUT))
                            continue;

                        throw new TerminalException($"Could not write to standard {_name}: {err}");
                    }
                }
            }
        }
    }

    public override TerminalReader StdIn { get; }

    public override TerminalWriter StdOut { get; }

    public override TerminalWriter StdError { get; }

    [SuppressMessage("Style", "IDE0052")]
    readonly PosixSignalRegistration _sigWinch;

    [SuppressMessage("Style", "IDE0052")]
    readonly PosixSignalRegistration _sigCont;

    readonly object _rawLock = new();

    protected UnixTerminalDriver()
    {
        StdIn = new UnixTerminalReader(this, STDIN_FILENO, "input");
        StdOut = new UnixTerminalWriter(this, STDOUT_FILENO, "output");
        StdError = new UnixTerminalWriter(this, STDERR_FILENO, "error");

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

    static bool IsRedirected(int handle)
    {
        return isatty(handle) == 0;
    }

    public override void GenerateBreakSignal(TerminalBreakSignal signal)
    {
        _ = kill(
            0,
            signal switch
            {
                TerminalBreakSignal.Interrupt => SIGINT,
                TerminalBreakSignal.Quit => SIGQUIT,
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
