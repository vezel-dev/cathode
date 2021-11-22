namespace System.Unix.Linux;

[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300")]
static unsafe class LinuxPInvoke
{
    [DllImport("c", SetLastError = true)]
    public static extern int poll(pollfd* fds, nuint nfds, int timeout);

    [DllImport("c", SetLastError = true)]
    public static extern int tcgetattr(int fd, out termios termios_p);

    [DllImport("c", SetLastError = true)]
    public static extern int tcsetattr(int fd, int optional_actions, in termios termios_p);
}
