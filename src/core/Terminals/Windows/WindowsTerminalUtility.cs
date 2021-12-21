using Windows.Win32.Foundation;

namespace System.Terminals.Windows;

static class WindowsTerminalUtility
{
    public static void ThrowIfUnexpected(string message)
    {
        var err = Marshal.GetLastPInvokeError();

        // See comments in UnixTerminalWriter for the error handling rationale.
        switch ((WIN32_ERROR)err)
        {
            case WIN32_ERROR.ERROR_HANDLE_EOF or WIN32_ERROR.ERROR_BROKEN_PIPE or WIN32_ERROR.ERROR_NO_DATA:
                break;
            default:
                throw new TerminalException($"{message}: {new Win32Exception(err).Message}");
        }
    }
}
