namespace Vezel.Cathode.Unix.Linux;

[StructLayout(LayoutKind.Sequential)]
[SuppressMessage("", "IDE1006")]
[SuppressMessage("", "SA1300")]
[SuppressMessage("", "SA1307")]
[SuppressMessage("", "SA1310")]
internal struct Termios
{
    public uint c_iflag;

    public uint c_oflag;

    public uint c_cflag;

    public uint c_lflag;

    public byte c_line;

    public unsafe fixed byte c_cc[32];

    public uint c_ispeed;

    public uint c_ospeed;
}
