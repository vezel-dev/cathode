namespace Vezel.Cathode.Unix.MacOS;

[StructLayout(LayoutKind.Sequential)]
[SuppressMessage("", "SA1307")]
[SuppressMessage("", "SA1310")]
internal struct Termios
{
    public nuint c_iflag;

    public nuint c_oflag;

    public nuint c_cflag;

    public nuint c_lflag;

    public unsafe fixed byte c_cc[20];

    public nuint c_ispeed;

    public nuint c_ospeed;
}
