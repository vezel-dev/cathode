namespace System.Unix.Linux;

[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310")]
static class LinuxConstants
{
    // c_iflag

    public const uint IGNBRK = 0x1;

    public const uint BRKINT = 0x2;

    public const uint PARMRK = 0x8;

    public const uint ISTRIP = 0x20;

    public const uint INLCR = 0x40;

    public const uint IGNCR = 0x80;

    public const uint ICRNL = 0x100;

    public const uint IXON = 0x400;

    // c_oflag

    public const uint OPOST = 0x1;

    // c_cflag

    public const uint CSIZE = 0x30;

    public const uint CS8 = 0x30;

    public const uint PARENB = 0x100;

    // c_lflag

    public const uint ISIG = 0x1;

    public const uint ICANON = 0x2;

    public const uint ECHO = 0x8;

    public const uint ECHONL = 0x40;

    public const uint IEXTEN = 0x8000;

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
}
