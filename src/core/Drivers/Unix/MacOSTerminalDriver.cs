using System.Unix;
using System.Unix.MacOS;
using static System.Unix.MacOS.MacOSPInvoke;
using static System.Unix.UnixPInvoke;

namespace System.Drivers.Unix;

sealed class MacOSTerminalDriver : UnixTerminalDriver
{
    // Keep this class in sync with the LinuxTerminalDriver class.

    public static MacOSTerminalDriver Instance { get; } = new();

    termios? _original;

    MacOSTerminalDriver()
    {
    }

    protected override TerminalSize? GetSize()
    {
        return ioctl(TerminalOut.Handle, TIOCGWINSZ, out var w) == 0 ? new(w.ws_col, w.ws_row) : null;
    }

    protected override void SetRawModeCore(bool raw, bool flush)
    {
        if (tcgetattr(TerminalOut.Handle, out var termios) == -1)
            throw new TerminalException("There is no terminal attached.");

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

        // Finally, enable/disable features that depend on cooked/raw mode.
        if (!raw)
        {
            termios.c_iflag |= iflag;
            termios.c_oflag |= oflag;
            termios.c_lflag |= lflag;
            termios.c_lflag &= ~(TOSTOP | NOKERNINFO);
        }
        else
        {
            termios.c_iflag &= ~iflag;
            termios.c_oflag &= ~oflag;
            termios.c_lflag &= ~lflag;
            termios.c_lflag |= TOSTOP | NOKERNINFO;
        }

        int ret;

        using (var guard = raw ? null : new PosixSignalGuard(PosixSignal.SIGTTOU))
        {
            while ((ret = tcsetattr(TerminalOut.Handle, flush ? TCSAFLUSH : TCSANOW, termios)) == -1 &&
                Marshal.GetLastPInvokeError() == EINTR)
            {
                // Retry in case we get interrupted by a signal. If we are trying to switch to cooked mode and we saw
                // SIGTTOU, it means we are a background process. We will trust that, by the time we actually read or
                // write anything, we will be in cooked mode.
                if (guard?.Signaled == true)
                    return;
            }
        }

        if (ret != 0)
            throw new TerminalException(
                $"Could not change raw mode setting: {new Win32Exception(Marshal.GetLastPInvokeError()).Message}");
    }

    public override void RestoreSettings()
    {
        if (_original is termios tios)
            _ = tcsetattr(TerminalOut.Handle, TCSAFLUSH, tios);
    }

    public override int OpenTerminalHandle(string name)
    {
        return open(name, O_RDWR | O_NOCTTY | O_CLOEXEC);
    }

    public override unsafe bool PollHandle(int error, int handle, short events)
    {
        if (error != EAGAIN)
            return false;

        var fd = new pollfd
        {
            fd = handle,
            events = events,
        };

        _ = poll(&fd, 1, -1);

        return true;
    }
}
