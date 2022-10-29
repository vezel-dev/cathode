namespace Vezel.Cathode.Unix;

[SuppressMessage("", "SA1300")]
[SuppressMessage("", "SA1310")]
internal static unsafe partial class UnixPInvoke
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

    [LibraryImport("c", SetLastError = true)]
    public static partial int isatty(int fildes);

    [LibraryImport("c", SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    public static partial int open(string path, int oflag);

    [LibraryImport("c", SetLastError = true)]
    public static partial nint read(int fildes, Span<byte> buf, nuint nbyte);

    [LibraryImport("c", SetLastError = true)]
    public static partial nint write(int fildes, ReadOnlySpan<byte> buf, nuint nbyte);

    [LibraryImport("c", SetLastError = true)]
    public static partial int ioctl(int fildes, nuint request, out Winsize argp);

    [LibraryImport("c", SetLastError = true)]
    public static partial int kill(int pid, int sig);
}
