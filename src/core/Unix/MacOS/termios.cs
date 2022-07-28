namespace Vezel.Cathode.Unix.MacOS;

[StructLayout(LayoutKind.Sequential)]
[SuppressMessage("", "IDE1006")]
[SuppressMessage("", "SA1300")]
[SuppressMessage("", "SA1307")]
[SuppressMessage("", "SA1310")]
internal struct termios
{
    public nuint c_iflag;

    public nuint c_oflag;

    public nuint c_cflag;

    public nuint c_lflag;

    public unsafe fixed byte c_cc[20];

    public nuint c_ispeed;

    public nuint c_ospeed;
}
