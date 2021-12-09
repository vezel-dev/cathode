using Windows.Win32.Foundation;

namespace System.Drivers.Windows;

static class WindowsTerminalUtility
{
    public static void ThrowIfUnexpected(string message)
    {
        // TODO: https://github.com/microsoft/CsWin32/issues/452
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
