using System.Unix;
using System.Unix.MacOS;
using static System.Unix.MacOS.MacOSPInvoke;
using static System.Unix.UnixPInvoke;

namespace System.Terminals.Unix.MacOS;

sealed class MacOSVirtualTerminal : UnixVirtualTerminal
{
    // Keep this class in sync with the LinuxVirtualTerminal class.

    public static MacOSVirtualTerminal Instance { get; } = new();

    termios? _original;

    MacOSVirtualTerminal()
    {
    }

    protected override TerminalSize? QuerySize()
    {
        return ioctl(TerminalOut.Handle, TIOCGWINSZ, out var w) == 0 ? new(w.ws_col, w.ws_row) : null;
    }

    protected override void SetRawModeCore(bool raw, bool flush)
    {
        if (tcgetattr(TerminalOut.Handle, out var termios) == -1)
            throw new TerminalNotAttachedException();

        // Stash away the original settings the first time we are successfully called.
        if (_original == null)
            _original = termios;

        // These values are usually the default, but we set them just to be safe since UnixTerminalReader would not
        // behave as expected by callers if these values differ.
        termios.c_cc[VTIME] = 0;
        termios.c_cc[VMIN] = 1;

        // Turn off some features that make little or no sense for virtual terminals.
        termios.c_iflag &= ~(IGNBRK | IGNPAR | PARMRK | INPCK | ISTRIP | IXOFF | IMAXBEL);
        termios.c_oflag &= ~(OFILL | OFDEL | NLDLY | CRDLY | TABDLY | BSDLY | VTDLY | FFDLY);
        termios.c_oflag |= NL0 | CR0 | TAB0 | BS0 | VT0 | FF0;
        termios.c_cflag &= ~(CSTOPB | PARENB | PARODD | HUPCL | CLOCAL | CRTSCTS | CDTR_IFLOW | CDSR_OFLOW | MDMBUF);
        termios.c_lflag &= ~(FLUSHO | EXTPROC);

        // Set up some sensible defaults.
        termios.c_iflag &= ~(IGNCR | INLCR | IXANY);
        termios.c_iflag |= IUTF8;
        termios.c_oflag &= ~(OCRNL | ONOCR | ONLRET | ALTWERASE);
        termios.c_cflag &= ~CSIZE;
        termios.c_cflag |= CS8 | CREAD;
        termios.c_lflag &= ~(ECHONL | NOFLSH | ECHOPRT | PENDIN);

        // TODO: What's up with ONOEOT? It is not listed in stty -a but does exist in termios.h.

        var iflag = BRKINT | ICRNL | IXON;
        var oflag = OPOST | ONLCR;
        var lflag = ISIG | ICANON | ECHO | ECHOE | ECHOK | ECHOCTL | ECHOKE | IEXTEN;

        // Finally, enable/disable features that depend on raw/cooked mode.
        if (raw)
        {
            termios.c_iflag &= ~iflag;
            termios.c_oflag &= ~oflag;
            termios.c_lflag &= ~lflag;
            termios.c_lflag |= TOSTOP | NOKERNINFO;
        }
        else
        {
            termios.c_iflag |= iflag;
            termios.c_oflag |= oflag;
            termios.c_lflag |= lflag;
            termios.c_lflag &= ~(TOSTOP | NOKERNINFO);
        }

        int ret;
        var err = 0;

        using (var guard = raw ? null : new PosixSignalGuard(PosixSignal.SIGTTOU))
        {
            while ((ret = tcsetattr(TerminalOut.Handle, flush ? TCSAFLUSH : TCSANOW, termios)) == -1 &&
                (err = Marshal.GetLastPInvokeError()) == EINTR)
            {
                // Retry in case we get interrupted by a signal. If we are trying to switch to cooked mode and we saw
                // SIGTTOU, it means we are a background process. We will trust that, by the time we actually read or
                // write anything, we will be in cooked mode.
                if (guard?.Signaled == true)
                    return;
            }
        }

        if (ret != 0)
            throw new TerminalConfigurationException(
                $"Could not change raw mode setting: {new Win32Exception(err).Message}");
    }

    public override int OpenTerminalHandle(string name)
    {
        return open(name, O_RDWR | O_NOCTTY | O_CLOEXEC);
    }

    public override unsafe (int ReadHandle, int WriteHandle) CreatePipePair()
    {
        var fds = stackalloc int[2];

        // Unfortunately, macOS lacks pipe2 so we have to use this approach which is prone to race conditions on fork.
        if (pipe(fds) == -1)
            return (-1, -1);

        static bool SetCloseOnExec(int handle)
        {
            var flags = fcntl(handle, F_GETFD);

            if (flags == -1)
                return false;

            flags |= O_CLOEXEC;

            return fcntl(handle, F_SETFD, flags) == 0;
        }

        if (!(SetCloseOnExec(fds[0]) && SetCloseOnExec(fds[1])))
        {
            _ = close(fds[0]);
            _ = close(fds[1]);

            return (-1, -1);
        }

        return (fds[0], fds[1]);
    }

    public override unsafe bool PollHandles(int? error, short events, Span<int> handles)
    {
        if (error is int err && err != EAGAIN)
            return false;

        Span<pollfd> fds = stackalloc pollfd[handles.Length];

        for (var i = 0; i < handles.Length; i++)
        {
            fds[i] = new pollfd
            {
                fd = handles[i],
                events = events,
                revents = 0, // Shut up CS0649.
            };
        }

        fixed (pollfd* p = &MemoryMarshal.GetReference(fds))
        {
            int ret;

            while ((ret = poll(p, (uint)fds.Length, -1)) == -1 && Marshal.GetLastPInvokeError() == EINTR)
            {
                // Retry in case we get interrupted by a signal.
            }

            if (ret == -1)
                return false;
        }

        for (var i = 0; i < handles.Length; i++)
            handles[i] = fds[i].revents;

        return true;
    }

    public override void DangerousRestoreSettings()
    {
        if (_original is termios tios)
            _ = tcsetattr(TerminalOut.Handle, TCSAFLUSH, tios);
    }
}
