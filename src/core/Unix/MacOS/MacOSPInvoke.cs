namespace System.Unix.MacOS;

[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300")]
static unsafe class MacOSPInvoke
{
    [DllImport("c", SetLastError = true)]
    public static extern int poll(pollfd* fds, uint nfds, int timeout);

    [DllImport("c", SetLastError = true)]
    public static extern int tcgetattr(int fildes, out termios termios_p);

    [DllImport("c", SetLastError = true)]
    public static extern int tcsetattr(int fildes, int optional_actions, in termios termios_p);
}
