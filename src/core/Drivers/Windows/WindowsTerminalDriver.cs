using Windows.Win32.Security;
using Windows.Win32.Storage.FileSystem;
using Windows.Win32.System.Console;
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

    (CONSOLE_MODE, CONSOLE_MODE)? _original;

    [SuppressMessage("Reliability", "CA2000")]
    WindowsTerminalDriver()
    {
        var inLock = new object();
        var outLock = new object();

        StandardIn = new(this, "standard input", GetStdHandle_SafeHandle(STD_HANDLE.STD_INPUT_HANDLE), new(), inLock);
        StandardOut = new(this, "standard output", GetStdHandle_SafeHandle(STD_HANDLE.STD_OUTPUT_HANDLE), outLock);
        StandardError = new(this, "standard error", GetStdHandle_SafeHandle(STD_HANDLE.STD_ERROR_HANDLE), outLock);

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

        TerminalIn = new(this, "terminal input", OpenConsoleHandle("CONIN$"), new(), inLock);
        TerminalOut = new(this, "terminal output", OpenConsoleHandle("CONOUT$"), outLock);

        // Input needs to be UTF-16, but we make it appear as if it is UTF-8 to users of the library. See the comments
        // in WindowsTerminalReader for the gory details.
        _ = SetConsoleCP((uint)Encoding.Unicode.CodePage);
        _ = SetConsoleOutputCP((uint)Encoding.UTF8.CodePage);

        try
        {
            // Start in cooked mode.
            SetRawModeCore(false, false);
        }
        catch (Exception e) when (e is TerminalNotAttachedException or TerminalConfigurationException)
        {
        }
    }

    protected override TerminalSize? GetSize()
    {
        return GetConsoleScreenBufferInfo(TerminalOut.Handle, out var info) ?
            new(info.srWindow.Right - info.srWindow.Left + 1, info.srWindow.Bottom - info.srWindow.Top + 1) : null;
    }

    public override void SendSignal(int pid, TerminalSignal signal)
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
            (uint)pid);
    }

    void SetRawModeCore(bool raw, bool flush)
    {
        if (!GetConsoleMode(TerminalIn.Handle, out var inMode) ||
            !GetConsoleMode(TerminalOut.Handle, out var outMode))
            throw new TerminalNotAttachedException();

        // Stash away the original modes the first time we are successfully called.
        if (_original == null)
            _original = (inMode, outMode);

        // Set up some sensible defaults.
        inMode &= ~(
            CONSOLE_MODE.ENABLE_WINDOW_INPUT |
            CONSOLE_MODE.ENABLE_MOUSE_INPUT |
            CONSOLE_MODE.ENABLE_QUICK_EDIT_MODE);
        inMode |=
            CONSOLE_MODE.ENABLE_INSERT_MODE |
            CONSOLE_MODE.ENABLE_EXTENDED_FLAGS |
            CONSOLE_MODE.ENABLE_VIRTUAL_TERMINAL_INPUT;
        outMode &= ~CONSOLE_MODE.ENABLE_LVB_GRID_WORLDWIDE;
        outMode |=
            CONSOLE_MODE.ENABLE_PROCESSED_OUTPUT |
            CONSOLE_MODE.ENABLE_WRAP_AT_EOL_OUTPUT |
            CONSOLE_MODE.ENABLE_VIRTUAL_TERMINAL_PROCESSING;

        var inExtra =
            CONSOLE_MODE.ENABLE_PROCESSED_INPUT |
            CONSOLE_MODE.ENABLE_LINE_INPUT |
            CONSOLE_MODE.ENABLE_ECHO_INPUT;
        var outExtra = CONSOLE_MODE.DISABLE_NEWLINE_AUTO_RETURN;

        // Enable/disable features that depend on cooked/raw mode.
        if (!raw)
        {
            inMode |= inExtra;
            outMode |= outExtra;
        }
        else
        {
            inMode &= ~inExtra;
            outMode &= ~outExtra;
        }

        if (!SetConsoleMode(TerminalIn.Handle, inMode) ||
            !SetConsoleMode(TerminalOut.Handle, outMode))
            throw new TerminalConfigurationException(
                $"Could not change raw mode setting: {new Win32Exception().Message}");

        if (flush && !FlushConsoleInputBuffer(TerminalIn.Handle))
            throw new TerminalConfigurationException(
                $"Could not flush input buffer: {new Win32Exception().Message}");
    }

    protected override void SetRawMode(bool raw)
    {
        SetRawModeCore(raw, true);
    }

    public override void RestoreSettings()
    {
        if (_original is var (inMode, outMode))
        {
            _ = SetConsoleMode(TerminalIn.Handle, inMode);
            _ = SetConsoleMode(TerminalOut.Handle, outMode);
        }
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
