namespace System.Unix.Linux;

[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300")]
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310")]
static unsafe class LinuxPInvoke
{
    // c_iflag

    public const uint IGNBRK = 0x1;

    public const uint BRKINT = 0x2;

    public const uint IGNPAR = 0x4;

    public const uint PARMRK = 0x8;

    public const uint INPCK = 0x10;

    public const uint ISTRIP = 0x20;

    public const uint INLCR = 0x40;

    public const uint IGNCR = 0x80;

    public const uint ICRNL = 0x100;

    public const uint IUCLC = 0x200;

    public const uint IXON = 0x400;

    public const uint IXANY = 0x800;

    public const uint IXOFF = 0x1000;

    public const uint IMAXBEL = 0x2000;

    public const uint IUTF8 = 0x4000;

    // c_oflag

    public const uint OPOST = 0x1;

    public const uint OLCUC = 0x2;

    public const uint ONLCR = 0x4;

    public const uint OCRNL = 0x8;

    public const uint ONOCR = 0x10;

    public const uint ONLRET = 0x20;

    public const uint OFILL = 0x40;

    public const uint OFDEL = 0x80;

    public const uint NLDLY = 0x100;

    public const uint NL0 = 0x0;

    public const uint CRDLY = 0x600;

    public const uint CR0 = 0x0;

    public const uint TABDLY = 0x1800;

    public const uint TAB0 = 0x0;

    public const uint BSDLY = 0x2000;

    public const uint BS0 = 0x0;

    public const uint VTDLY = 0x4000;

    public const uint VT0 = 0x0;

    public const uint FFDLY = 0x8000;

    public const uint FF0 = 0x0;

    // c_cflag

    public const uint CSTOPB = 0x40;

    public const uint CREAD = 0x80;

    public const uint PARENB = 0x100;

    public const uint PARODD = 0x200;

    public const uint HUPCL = 0x400;

    public const uint CLOCAL = 0x800;

    public const uint CMSPAR = 0x40000000;

    public const uint CRTSCTS = 0x80000000;

    public const uint CSIZE = 0x30;

    public const uint CS8 = 0x30;

    // c_lflag

    public const uint ISIG = 0x1;

    public const uint ICANON = 0x2;

    public const uint XCASE = 0x4;

    public const uint ECHO = 0x8;

    public const uint ECHOE = 0x10;

    public const uint ECHOK = 0x20;

    public const uint ECHONL = 0x40;

    public const uint NOFLSH = 0x80;

    public const uint TOSTOP = 0x100;

    public const uint ECHOCTL = 0x200;

    public const uint ECHOPRT = 0x400;

    public const uint ECHOKE = 0x800;

    public const uint FLUSHO = 0x1000;

    public const uint PENDIN = 0x4000;

    public const uint IEXTEN = 0x8000;

    public const uint EXTPROC = 0x10000;

    // c_cc

    public const int VTIME = 5;

    public const int VMIN = 6;

    // flags

    public const int O_RDWR = 0x2;

    public const int O_NOCTTY = 0x100;

    public const int O_CLOEXEC = 0x80000;

    // request

    public const nuint TIOCGWINSZ = 0x5413;

    // errno

    public const int EAGAIN = 11;

    [DllImport("c", SetLastError = true)]
    public static extern int poll(pollfd* fds, nuint nfds, int timeout);

    [DllImport("c", SetLastError = true)]
    public static extern int tcgetattr(int fildes, out termios termios_p);

    [DllImport("c", SetLastError = true)]
    public static extern int tcsetattr(int fildes, int optional_actions, in termios termios_p);
}
