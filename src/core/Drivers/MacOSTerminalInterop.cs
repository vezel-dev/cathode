using Mono.Unix.Native;

namespace System.Drivers;

[SuppressMessage("Style", "IDE1006")]
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300")]
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307")]
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310")]
sealed class MacOSTerminalInterop : UnixTerminalInterop
{
    struct termios
    {
        public nuint c_iflag;

        public nuint c_oflag;

        public nuint c_cflag;

        public nuint c_lflag;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public byte[] c_cc;

        public nuint c_ispeed;

        public nuint c_ospeed;
    }

    struct winsize
    {
        public ushort ws_row;

        public ushort ws_col;

        public ushort ws_xpixel;

        public ushort ws_ypixel;
    }

    // c_iflag

    const nuint IGNBRK = 0x1;

    const nuint BRKINT = 0x2;

    const nuint PARMRK = 0x8;

    const nuint ISTRIP = 0x20;

    const nuint INLCR = 0x40;

    const nuint IGNCR = 0x80;

    const nuint ICRNL = 0x100;

    const nuint IXON = 0x200;

    // c_oflag

    const nuint OPOST = 0x1;

    // c_cflag

    const nuint CSIZE = 0x300;

    const nuint CS8 = 0x300;

    const nuint PARENB = 0x1000;

    // c_lflag

    const nuint ECHO = 0x8;

    const nuint ECHONL = 0x10;

    const nuint ISIG = 0x80;

    const nuint ICANON = 0x100;

    const nuint IEXTEN = 0x400;

    // c_cc

    const int VMIN = 16;

    const int VTIME = 17;

    // optional_actions

    const int TCSANOW = 0;

    const int TCSAFLUSH = 2;

    // request

    const nuint TIOCGWINSZ = 0x40087468;

    [DllImport("c", SetLastError = true)]
    static extern int tcgetattr(int fd, out termios termios_p);

    [DllImport("c", SetLastError = true)]
    static extern int tcsetattr(int fd, int optional_actions, in termios termios_p);

    [DllImport("c", SetLastError = true)]
    static extern int ioctl(int fildes, nuint request, out winsize argp);

    public static MacOSTerminalInterop Instance { get; } = new();

    public override TerminalSize? Size =>
        ioctl(UnixTerminalDriver.OutHandle, TIOCGWINSZ, out var w) == 0 ? new(w.ws_col, w.ws_row) : null;

    readonly termios? _original;

    termios? _current;

    MacOSTerminalInterop()
    {
        if (tcgetattr(UnixTerminalDriver.InHandle, out var settings) == 0)
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

    public override void RefreshSettings()
    {
        // This call can fail if the terminal is detached, but that is OK.
        if (_current is termios c)
            _ = UpdateSettings(TCSANOW, c);
    }

    public override bool SetRawMode(bool raw, bool discard)
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

        return UpdateSettings(discard ? TCSAFLUSH : TCSANOW, settings) ? true :
            throw new TerminalException(
                $"Could not change raw mode setting: {Stdlib.strerror(Stdlib.GetLastError())}");
    }

    bool UpdateSettings(int mode, in termios settings)
    {
        int ret;

        while ((ret = tcsetattr(UnixTerminalDriver.InHandle, mode, settings)) == -1 &&
            Stdlib.GetLastError() == Errno.EINTR)
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
}
