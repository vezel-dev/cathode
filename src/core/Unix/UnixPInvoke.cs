namespace System.Unix;

[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300")]
static unsafe class UnixPInvoke
{
    [DllImport("c", SetLastError = true)]
    public static extern int isatty(int fd);

    [DllImport("c", CharSet = CharSet.Ansi, SetLastError = true)]
    [SuppressMessage("Globalization", "CA2101")] // TODO: https://github.com/dotnet/roslyn-analyzers/issues/5479
    public static extern int open(string pathname, int flags);

    [DllImport("c", SetLastError = true)]
    public static extern nint read(int fd, void* buf, nuint count);

    [DllImport("c", SetLastError = true)]
    public static extern nint write(int fd, void* buf, nuint count);

    [DllImport("c", SetLastError = true)]
    public static extern int ioctl(int fd, nuint request, out winsize argp);

    [DllImport("c", SetLastError = true)]
    public static extern int kill(int pid, int sig);
}
