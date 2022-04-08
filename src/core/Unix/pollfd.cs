namespace Vezel.Cathode.Unix;

[SuppressMessage("Style", "IDE1006")]
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300")]
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307")]
struct pollfd
{
    public int fd;

    public short events;

    public short revents;
}
