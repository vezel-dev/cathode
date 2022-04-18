namespace Vezel.Cathode.Unix;

[SuppressMessage("", "SA1300")]
[SuppressMessage("", "SA1310")]
static unsafe class UnixPInvoke
{
    // fd

    public const int STDIN_FILENO = 0;

    public const int STDOUT_FILENO = 1;

    public const int STDERR_FILENO = 2;

    // events, revents

    public const short POLLIN = 0x1;

    public const short POLLOUT = 0x4;

    // optional_actions

    public const int TCSANOW = 0;

    public const int TCSAFLUSH = 2;

    // sig

    public const int SIGHUP = 1;

    public const int SIGINT = 2;

    public const int SIGQUIT = 3;

    public const int SIGTERM = 15;

    // errno

    public const int EINTR = 4;

    public const int EPIPE = 32;

    [DllImport("c", SetLastError = true)]
    public static extern int isatty(int fildes);

    [DllImport("c", CharSet = CharSet.Ansi, SetLastError = true)]
    [SuppressMessage("", "CA2101")] // TODO: https://github.com/dotnet/roslyn-analyzers/issues/5479
    public static extern int open(string path, int oflag);

    [DllImport("c", SetLastError = true)]
    public static extern nint read(int fildes, void* buf, nuint nbyte);

    [DllImport("c", SetLastError = true)]
    public static extern nint write(int fildes, void* buf, nuint nbyte);

    [DllImport("c", SetLastError = true)]
    public static extern int ioctl(int fildes, nuint request, out winsize argp);

    [DllImport("c", SetLastError = true)]
    public static extern int kill(int pid, int sig);
}
