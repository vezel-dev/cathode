namespace Vezel.Cathode.Unix;

[SuppressMessage("", "IDE1006")]
[SuppressMessage("", "SA1300")]
[SuppressMessage("", "SA1307")]
[SuppressMessage("", "SA1310")]
struct winsize
{
    public ushort ws_row;

    public ushort ws_col;

    public ushort ws_xpixel;

    public ushort ws_ypixel;
}
