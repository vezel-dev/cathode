namespace Vezel.Cathode.Unix;

[StructLayout(LayoutKind.Sequential)]
[SuppressMessage("", "SA1307")]
[SuppressMessage("", "SA1310")]
internal struct Winsize
{
    public ushort ws_row;

    public ushort ws_col;

    public ushort ws_xpixel;

    public ushort ws_ypixel;
}
