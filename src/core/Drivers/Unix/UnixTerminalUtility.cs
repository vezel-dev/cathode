using static System.Unix.UnixPInvoke;

namespace System.Drivers.Unix;

static class UnixTerminalUtility
{
    public static bool IsRedirected(int handle)
    {
        return isatty(handle) == 0;
    }
}
