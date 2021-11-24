using Windows.Win32.Foundation;
using static Windows.Win32.Constants;
using static Windows.Win32.WindowsPInvoke;

namespace System.Drivers.Windows;

static class WindowsTerminalUtility
{
    public static unsafe bool IsHandleValid(SafeHandle handle, bool write)
    {
        if (handle.IsInvalid)
            return false;

        if (write)
        {
            uint dummy = 42;

            return WriteFile(handle, &dummy, 0, &dummy, null);
        }

        return true;
    }

    public static bool IsHandleRedirected(SafeHandle handle)
    {
        return GetFileType(handle) != FILE_TYPE_CHAR || !GetConsoleMode(handle, out _);
    }

    public static void ThrowIfUnexpected(string message)
    {
        // TODO: https://github.com/microsoft/CsWin32/issues/452
        var err = (WIN32_ERROR)Marshal.GetLastSystemError();

        // See comments in UnixTerminalWriter for the error handling rationale.
        switch (err)
        {
            case WIN32_ERROR.ERROR_HANDLE_EOF or WIN32_ERROR.ERROR_BROKEN_PIPE or WIN32_ERROR.ERROR_NO_DATA:
                break;
            default:
                throw new TerminalException($"{message}: {err}");
        }
    }
}
