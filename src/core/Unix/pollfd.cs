namespace Vezel.Cathode.Unix;

[SuppressMessage("", "IDE1006")]
[SuppressMessage("", "SA1300")]
[SuppressMessage("", "SA1307")]
struct pollfd
{
    public int fd;

    public short events;

    public short revents;
}
