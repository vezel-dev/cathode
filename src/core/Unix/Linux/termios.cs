namespace Vezel.Cathode.Unix.Linux;

[SuppressMessage("Style", "IDE1006")]
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300")]
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307")]
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310")]
struct termios
{
    public uint c_iflag;

    public uint c_oflag;

    public uint c_cflag;

    public uint c_lflag;

    public byte c_line;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    public byte[] c_cc;

    public uint c_ispeed;

    public uint c_ospeed;
}
