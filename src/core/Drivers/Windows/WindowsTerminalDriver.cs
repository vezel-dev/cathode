using Windows.Win32.Foundation;
using Windows.Win32.Security;
using Windows.Win32.Storage.FileSystem;
using Windows.Win32.System.Console;
using static Windows.Win32.Constants;
using static Windows.Win32.WindowsPInvoke;

namespace System.Drivers.Windows;

sealed class WindowsTerminalDriver : TerminalDriver<SafeHandle>
{
    public static WindowsTerminalDriver Instance { get; } = new();

    public override WindowsTerminalReader StandardIn { get; }

    public override WindowsTerminalWriter StandardOut { get; }

    public override WindowsTerminalWriter StandardError { get; }

    public override WindowsTerminalReader TerminalIn { get; }

    public override WindowsTerminalWriter TerminalOut { get; }

    [SuppressMessage("Reliability", "CA2000")]
    WindowsTerminalDriver()
    {
        var inLock = new object();
        var outLock = new object();

        StandardIn = new(this, "standard input", GetStdHandle_SafeHandle(STD_HANDLE.STD_INPUT_HANDLE), inLock);
        StandardOut = new(this, "standard output", GetStdHandle_SafeHandle(STD_HANDLE.STD_OUTPUT_HANDLE), outLock);
        StandardError = new(this, "standard error", GetStdHandle_SafeHandle(STD_HANDLE.STD_ERROR_HANDLE), new());

        static SafeHandle OpenConsoleHandle(string name)
        {
            return CreateFileW(
                name,
                FILE_ACCESS_FLAGS.FILE_GENERIC_READ | FILE_ACCESS_FLAGS.FILE_GENERIC_WRITE,
                FILE_SHARE_MODE.FILE_SHARE_READ | FILE_SHARE_MODE.FILE_SHARE_WRITE,
                new SECURITY_ATTRIBUTES
                {
                    bInheritHandle = true,
                },
                FILE_CREATION_DISPOSITION.OPEN_EXISTING,
                0,
                null);
        }

        TerminalIn = new(this, "terminal input", OpenConsoleHandle("CONIN$"), inLock);
        TerminalOut = new(this, "terminal output", OpenConsoleHandle("CONOUT$"), outLock);

        // Input needs to be UTF-16, but we make it appear as if it is UTF-8 to users of the library. See the comments
        // in WindowsTerminalReader for the gory details.
        _ = SetConsoleCP((uint)Encoding.Unicode.CodePage);
        _ = SetConsoleOutputCP((uint)Encoding.UTF8.CodePage);

        // Start in cooked mode.
        _ = AddConsoleMode(
            TerminalIn.Handle,
            CONSOLE_MODE.ENABLE_PROCESSED_INPUT |
            CONSOLE_MODE.ENABLE_LINE_INPUT |
            CONSOLE_MODE.ENABLE_ECHO_INPUT |
            CONSOLE_MODE.ENABLE_INSERT_MODE |
            CONSOLE_MODE.ENABLE_EXTENDED_FLAGS |
            CONSOLE_MODE.ENABLE_VIRTUAL_TERMINAL_INPUT);
        _ = AddConsoleMode(
            TerminalOut.Handle,
            CONSOLE_MODE.ENABLE_PROCESSED_OUTPUT |
            CONSOLE_MODE.ENABLE_WRAP_AT_EOL_OUTPUT |
            CONSOLE_MODE.ENABLE_VIRTUAL_TERMINAL_PROCESSING);
    }

    static bool AddConsoleMode(SafeHandle handle, CONSOLE_MODE mode)
    {
        return GetConsoleMode(handle, out var m) && SetConsoleMode(handle, m | mode);
    }

    static bool RemoveConsoleMode(SafeHandle handle, CONSOLE_MODE mode)
    {
        return GetConsoleMode(handle, out var m) && SetConsoleMode(handle, m & ~mode);
    }

    protected override TerminalSize? GetSize()
    {
        return GetConsoleScreenBufferInfo(TerminalOut.Handle, out var info) ?
            new(info.srWindow.Right - info.srWindow.Left + 1, info.srWindow.Bottom - info.srWindow.Top + 1) : null;
    }

    public override void GenerateSignal(TerminalSignal signal)
    {
        _ = GenerateConsoleCtrlEvent(
            signal switch
            {
                TerminalSignal.Close => CTRL_CLOSE_EVENT,
                TerminalSignal.Interrupt => CTRL_C_EVENT,
                TerminalSignal.Quit => CTRL_BREAK_EVENT,
                TerminalSignal.Terminate => CTRL_SHUTDOWN_EVENT,
                _ => throw new ArgumentOutOfRangeException(nameof(signal)),
            },
            0);
    }

    protected override void SetRawMode(bool raw)
    {
        if (!TerminalIn.IsInteractive || !TerminalOut.IsInteractive)
            throw new TerminalException("There is no terminal attached.");

        var inMode =
            CONSOLE_MODE.ENABLE_PROCESSED_INPUT |
            CONSOLE_MODE.ENABLE_LINE_INPUT |
            CONSOLE_MODE.ENABLE_ECHO_INPUT;
        var outMode =
            CONSOLE_MODE.DISABLE_NEWLINE_AUTO_RETURN;

        if (!(raw ? RemoveConsoleMode(TerminalIn.Handle, inMode) && RemoveConsoleMode(TerminalOut.Handle, outMode) :
            AddConsoleMode(TerminalIn.Handle, inMode) && AddConsoleMode(TerminalOut.Handle, outMode)))
            throw new TerminalException(
                $"Could not change raw mode setting: {(WIN32_ERROR)Marshal.GetLastSystemError()}");

        if (!FlushConsoleInputBuffer(StandardIn.Handle))
            throw new TerminalException(
                $"Could not flush input buffer: {(WIN32_ERROR)Marshal.GetLastSystemError()}");
    }

    public override unsafe bool IsHandleValid(SafeHandle handle, bool write)
    {
        if (handle.IsInvalid)
            return false;

        // Apparently, for Windows GUI programs, the standard I/O handles will appear to be valid (i.e. not -1 or 0) but
        // will not actually be usable. So do a zero-byte write to figure out if the handle is actually valid.
        if (write)
        {
            var dummy = 42u;

            return WriteFile(handle, &dummy, 0, &dummy, null);
        }

        return true;
    }

    public override bool IsHandleInteractive(SafeHandle handle)
    {
        // Note that this also returns true for invalid handles.
        return GetFileType(handle) == FILE_TYPE_CHAR && GetConsoleMode(handle, out _);
    }
}
