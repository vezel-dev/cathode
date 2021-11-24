namespace System.Unix.MacOS;

static class MacOSConstants
{
    // c_iflag

    public const nuint IGNBRK = 0x1;

    public const nuint BRKINT = 0x2;

    public const nuint PARMRK = 0x8;

    public const nuint ISTRIP = 0x20;

    public const nuint INLCR = 0x40;

    public const nuint IGNCR = 0x80;

    public const nuint ICRNL = 0x100;

    public const nuint IXON = 0x200;

    // c_oflag

    public const nuint OPOST = 0x1;

    // c_cflag

    public const nuint CSIZE = 0x300;

    public const nuint CS8 = 0x300;

    public const nuint PARENB = 0x1000;

    // c_lflag

    public const nuint ECHO = 0x8;

    public const nuint ECHONL = 0x10;

    public const nuint ISIG = 0x80;

    public const nuint ICANON = 0x100;

    public const nuint IEXTEN = 0x400;

    // c_cc

    public const int VMIN = 16;

    public const int VTIME = 17;

    // request

    public const nuint TIOCGWINSZ = 0x40087468;

    // errno

    public const int EAGAIN = 35;
}
