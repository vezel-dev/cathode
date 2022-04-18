namespace Vezel.Cathode.Unix.MacOS;

[SuppressMessage("", "IDE1006")]
[SuppressMessage("", "SA1300")]
[SuppressMessage("", "SA1307")]
[SuppressMessage("", "SA1310")]
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
