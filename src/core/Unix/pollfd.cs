namespace Vezel.Cathode.Unix;

[StructLayout(LayoutKind.Sequential)]
[SuppressMessage("", "SA1307")]
internal struct Pollfd
{
    public int fd;

    public short events;

    public short revents;
}
