using System.Unix;
using System.Unix.Linux;
using static System.Unix.Linux.LinuxConstants;
using static System.Unix.Linux.LinuxPInvoke;
using static System.Unix.UnixConstants;
using static System.Unix.UnixPInvoke;

namespace System.Drivers.Unix;

sealed class LinuxTerminalDriver : UnixTerminalDriver
{
    public static LinuxTerminalDriver Instance { get; } = new();

    readonly termios? _original;

    termios? _current;

    LinuxTerminalDriver()
    {
        if (tcgetattr(STDIN_FILENO, out var settings) == 0)
        {
            // These values are usually the default, but we set them just to be safe.
            settings.c_cc[VTIME] = 0;
            settings.c_cc[VMIN] = 1;

            _original = settings;

            // We might get really unlucky and fail to apply the settings right after the call above. We should still
            // assign _current so we can apply it later.
            if (!UpdateSettings(TCSANOW, settings))
                _current = settings;
        }
    }

    bool UpdateSettings(int mode, in termios settings)
    {
        int ret;

        while ((ret = tcsetattr(STDIN_FILENO, mode, settings)) == -1 && Marshal.GetLastPInvokeError() == EINTR)
        {
            // Retry in case we get interrupted by a signal.
        }

        if (ret == 0)
        {
            _current = settings;

            return true;
        }

        return false;
    }

    protected override TerminalSize? GetSize()
    {
        return ioctl(STDOUT_FILENO, TIOCGWINSZ, out var w) == 0 ? new(w.ws_col, w.ws_row) : null;
    }

    protected override void RefreshSettings()
    {
        // This call can fail if the terminal is detached, but that is OK.
        if (_current is termios c)
            _ = UpdateSettings(TCSANOW, c);
    }

    protected override bool SetRawModeCore(bool raw)
    {
        if (_original is not termios settings)
            return false;

        if (raw)
        {
            settings.c_iflag &= ~(IGNBRK | BRKINT | PARMRK | ISTRIP | INLCR | IGNCR | ICRNL | IXON);
            settings.c_oflag &= ~OPOST;
            settings.c_cflag &= ~(CSIZE | PARENB);
            settings.c_cflag |= CS8;
            settings.c_lflag &= ~(ISIG | ICANON | ECHO | ECHONL | IEXTEN);
        }

        return UpdateSettings(TCSAFLUSH, settings) ?
            true : throw new TerminalException($"Could not change raw mode setting: {Marshal.GetLastPInvokeError()}");
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