namespace Vezel.Cathode.Unix;

[SuppressMessage("Style", "IDE1006")]
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300")]
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307")]
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310")]
struct winsize
{
    public ushort ws_row;

    public ushort ws_col;

    public ushort ws_xpixel;

    public ushort ws_ypixel;
}
