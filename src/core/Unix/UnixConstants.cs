namespace System.Unix;

[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310")]
static class UnixConstants
{
    // fd

    public const int STDIN_FILENO = 0;

    public const int STDOUT_FILENO = 1;

    public const int STDERR_FILENO = 2;

    // events, revents

    public const short POLLIN = 0x0001;

    public const short POLLOUT = 0x0004;

    // optional_actions

    public const int TCSANOW = 0;

    public const int TCSAFLUSH = 2;

    // sig

    public const int SIGINT = 2;

    public const int SIGQUIT = 3;

    // errno

    public const int EINTR = 4;

    public const int EPIPE = 32;
}
