namespace Vezel.Cathode.Unix;

[StructLayout(LayoutKind.Sequential)]
[SuppressMessage("", "IDE1006")]
[SuppressMessage("", "SA1300")]
[SuppressMessage("", "SA1307")]
internal struct Pollfd
{
    public int fd;

    public short events;

    public short revents;
}
