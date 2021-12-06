namespace System.Unix.MacOS;

[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300")]
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310")]
static unsafe class MacOSPInvoke
{
    // c_iflag

    public const nuint IGNBRK = 0x1;

    public const nuint BRKINT = 0x2;

    public const nuint IGNPAR = 0x4;

    public const nuint PARMRK = 0x8;

    public const nuint INPCK = 0x10;

    public const nuint ISTRIP = 0x20;

    public const nuint INLCR = 0x40;

    public const nuint IGNCR = 0x80;

    public const nuint ICRNL = 0x100;

    public const nuint IXON = 0x200;

    public const nuint IXOFF = 0x400;

    public const nuint IXANY = 0x800;

    public const nuint IMAXBEL = 0x2000;

    public const nuint IUTF8 = 0x4000;

    // c_oflag

    public const nuint OPOST = 0x1;

    public const nuint ONLCR = 0x2;

    public const nuint OCRNL = 0x10;

    public const nuint ONOCR = 0x20;

    public const nuint ONLRET = 0x40;

    public const nuint OFILL = 0x80;

    public const nuint ALTWERASE = 0x200;

    public const nuint OFDEL = 0x20000;

    public const nuint NLDLY = 0x300;

    public const nuint NL0 = 0x0;

    public const nuint TABDLY = 0xc04;

    public const nuint TAB0 = 0x0;

    public const nuint CRDLY = 0x3000;

    public const nuint CR0 = 0x0;

    public const nuint FFDLY = 0x4000;

    public const nuint FF0 = 0x0;

    public const nuint BSDLY = 0x8000;

    public const nuint BS0 = 0x0;

    public const nuint VTDLY = 0x10000;

    public const nuint VT0 = 0x0;

    // c_cflag

    public const nuint CSTOPB = 0x400;

    public const nuint CREAD = 0x800;

    public const nuint PARENB = 0x1000;

    public const nuint PARODD = 0x2000;

    public const nuint HUPCL = 0x4000;

    public const nuint CLOCAL = 0x8000;

    public const nuint CRTSCTS = 0x30000;

    public const nuint CDTR_IFLOW = 0x40000;

    public const nuint CDSR_OFLOW = 0x80000;

    public const nuint MDMBUF = 0x100000;

    public const nuint CSIZE = 0x300;

    public const nuint CS8 = 0x300;

    // c_lflag

    public const nuint ECHOKE = 0x1;

    public const nuint ECHOE = 0x2;

    public const nuint ECHOK = 0x4;

    public const nuint ECHO = 0x8;

    public const nuint ECHONL = 0x10;

    public const nuint ECHOPRT = 0x20;

    public const nuint ECHOCTL = 0x40;

    public const nuint ISIG = 0x80;

    public const nuint ICANON = 0x100;

    public const nuint IEXTEN = 0x400;

    public const nuint EXTPROC = 0x800;

    public const nuint TOSTOP = 0x400000;

    public const nuint FLUSHO = 0x800000;

    public const nuint NOKERNINFO = 0x2000000;

    public const nuint PENDIN = 0x20000000;

    public const nuint NOFLSH = 0x80000000;

    // c_cc

    public const int VMIN = 16;

    public const int VTIME = 17;

    // flags

    public const int O_RDWR = 0x2;

    public const int O_NOCTTY = 0x20000;

    public const int O_CLOEXEC = 0x1000000;

    // request

    public const nuint TIOCGWINSZ = 0x40087468;

    // errno

    public const int EAGAIN = 35;

    [DllImport("c", SetLastError = true)]
    public static extern int poll(pollfd* fds, uint nfds, int timeout);

    [DllImport("c", SetLastError = true)]
    public static extern int tcgetattr(int fildes, out termios termios_p);

    [DllImport("c", SetLastError = true)]
    public static extern int tcsetattr(int fildes, int optional_actions, in termios termios_p);
}
