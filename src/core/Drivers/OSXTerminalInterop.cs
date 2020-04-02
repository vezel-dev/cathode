using System.Runtime.InteropServices;
using Mono.Unix.Native;

namespace System.Drivers
{
    sealed class OSXTerminalInterop : IUnixTerminalInterop
    {
#pragma warning disable IDE1006

        struct termios
        {
            public UIntPtr c_iflag;

            public UIntPtr c_oflag;

            public UIntPtr c_cflag;

            public UIntPtr c_lflag;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public byte[] c_cc;

            public UIntPtr c_ispeed;

            public UIntPtr c_ospeed;
        }

        struct winsize
        {
            public ushort ws_row;

            public ushort ws_col;

            public ushort ws_xpixel;

            public ushort ws_ypixel;
        }

        // c_iflag

        const uint IGNBRK = 0x1;

        const uint BRKINT = 0x2;

        const uint PARMRK = 0x8;

        const uint ISTRIP = 0x20;

        const uint INLCR = 0x40;

        const uint IGNCR = 0x80;

        const uint ICRNL = 0x100;

        const uint IXON = 0x200;

        // c_oflag

        const uint OPOST = 0x1;

        // c_cflag

        const uint CSIZE = 0x300;

        const uint CS8 = 0x300;

        const uint PARENB = 0x1000;

        // c_lflag

        const uint ECHO = 0x8;

        const uint ECHONL = 0x10;

        const uint ISIG = 0x80;

        const uint ICANON = 0x100;

        const uint IEXTEN = 0x400;

        // c_cc

        const int VMIN = 16;

        const int VTIME = 17;

        // optional_actions

        const int TCSANOW = 0;

        const int TCSAFLUSH = 2;

        // request

        const uint TIOCGWINSZ = 0x40087468;

        [DllImport("libc")]
        static extern int tcgetattr(int fd, out termios termios_p);

        [DllImport("libc")]
        static extern int tcsetattr(int fd, int optional_actions, in termios termios_p);

        [DllImport("libc")]
        static extern int ioctl(int fildes, UIntPtr request, out winsize argp);

#pragma warning restore IDE1006

        public static OSXTerminalInterop Instance { get; } = new OSXTerminalInterop();

        public TerminalSize? Size =>
            ioctl(UnixTerminalDriver.OutHandle, (UIntPtr)TIOCGWINSZ, out var w) == 0 ?
                new TerminalSize(w.ws_col, w.ws_row) : default;

        readonly termios? _original;

        termios? _current;

        OSXTerminalInterop()
        {
            if (tcgetattr(UnixTerminalDriver.InHandle, out var settings) == 0)
            {
                // These values are usually the default, but we set them just to be safe.
                settings.c_cc[VTIME] = 0;
                settings.c_cc[VMIN] = 1;

                _original = settings;

                // We might get really unlucky and fail to apply the settings right after the call
                // above. We should still assign _current so we can apply it later.
                if (!UpdateSettings(TCSANOW, settings))
                    _current = settings;
            }
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

        public void RefreshSettings()
        {
            // This call can fail if the terminal is detached, but that is OK.
            if (_current is termios c)
                _ = UpdateSettings(TCSANOW, c);
        }

        public bool SetRawMode(bool raw, bool discard)
        {
            if (!(_original is termios settings))
                return false;

            if (raw)
            {
                var iflag = (uint)settings.c_iflag;
                var oflag = (uint)settings.c_oflag;
                var cflag = (uint)settings.c_cflag;
                var lflag = (uint)settings.c_lflag;

                iflag &= ~(IGNBRK | BRKINT | PARMRK | ISTRIP | INLCR | IGNCR | ICRNL | IXON);
                oflag &= ~OPOST;
                cflag &= ~(CSIZE | PARENB);
                cflag |= CS8;
                lflag &= ~(ISIG | ICANON | ECHO | ECHONL | IEXTEN);

                settings.c_iflag = (UIntPtr)iflag;
                settings.c_oflag = (UIntPtr)oflag;
                settings.c_cflag = (UIntPtr)cflag;
                settings.c_lflag = (UIntPtr)lflag;
            }

            return UpdateSettings(discard ? TCSAFLUSH : TCSANOW, settings) ? true :
                throw new TerminalException(
                    $"Could not change raw mode setting: {Stdlib.strerror(Stdlib.GetLastError())}");
        }
    }
}
