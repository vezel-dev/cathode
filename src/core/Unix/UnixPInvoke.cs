namespace System.Unix;

[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300")]
static unsafe class UnixPInvoke
{
    [DllImport("c", SetLastError = true)]
    public static extern int isatty(int fd);

    [DllImport("c", SetLastError = true)]
    public static extern nint read(int fd, void* buf, nuint count);

    [DllImport("c", SetLastError = true)]
    public static extern nint write(int fd, void* buf, nuint count);

    [DllImport("c", SetLastError = true)]
    public static extern int ioctl(int fd, nuint request, out winsize argp);

    [DllImport("c", SetLastError = true)]
    public static extern int kill(int pid, int sig);
}
