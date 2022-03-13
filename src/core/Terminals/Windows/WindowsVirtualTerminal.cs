using Windows.Win32.Security;
using Windows.Win32.Storage.FileSystem;
using Windows.Win32.System.Console;
using static Windows.Win32.WindowsPInvoke;

namespace Cathode.Terminals.Windows;

sealed class WindowsVirtualTerminal : NativeVirtualTerminal<SafeHandle>
{
    sealed class ConsoleState
    {
        public CONSOLE_MODE InMode { get; }

        public CONSOLE_MODE OutMode { get; }

        public uint InCodePage { get; }

        public uint OutCodePage { get; }

        public ConsoleState(CONSOLE_MODE inMode, CONSOLE_MODE outMode, uint inCodePage, uint outCodePage)
        {
            InMode = inMode;
            OutMode = outMode;
            InCodePage = inCodePage;
            OutCodePage = outCodePage;
        }
    }

    public override event Action? Resumed
    {
        add
        {
            // Windows does not have a SIGSTOP/SIGCONT concept.
        }
        remove
        {
        }
    }

    public static WindowsVirtualTerminal Instance { get; } = new();

    public override WindowsTerminalReader StandardIn { get; }

    public override WindowsTerminalWriter StandardOut { get; }

    public override WindowsTerminalWriter StandardError { get; }

    public override WindowsTerminalReader TerminalIn { get; }

    public override WindowsTerminalWriter TerminalOut { get; }

    ConsoleState? _original;

    [SuppressMessage("Reliability", "CA2000")]
    WindowsVirtualTerminal()
    {
        var inLock = new SemaphoreSlim(1, 1);
        var outLock = new SemaphoreSlim(1, 1);

        StandardIn = new(this, "standard input", GetStdHandle_SafeHandle(STD_HANDLE.STD_INPUT_HANDLE), inLock);
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

        TerminalIn = new(this, "terminal input", OpenConsoleHandle("CONIN$"), inLock);
        TerminalOut = new(this, "terminal output", OpenConsoleHandle("CONOUT$"), outLock);

        try
        {
            // Start in cooked mode.
            SetModeCore(false, false);
        }
        catch (Exception e) when (e is TerminalNotAttachedException or TerminalConfigurationException)
        {
        }
    }

    protected override TerminalSize? QuerySize()
    {
        return GetConsoleScreenBufferInfo(TerminalOut.Handle, out var info) ?
            new(info.srWindow.Right - info.srWindow.Left + 1, info.srWindow.Bottom - info.srWindow.Top + 1) : null;
    }

    public override void GenerateSignal(TerminalSignal signal)
    {
        using var guard = Control.Guard();

        _ = GenerateConsoleCtrlEvent(
            signal switch
            {
                TerminalSignal.Interrupt => CTRL_C_EVENT,
                TerminalSignal.Quit => CTRL_BREAK_EVENT,
                TerminalSignal.Close or TerminalSignal.Terminate => throw new PlatformNotSupportedException(),
                _ => throw new ArgumentOutOfRangeException(nameof(signal)),
            },
            0);
    }

    void SetModeCore(bool raw, bool flush)
    {
        uint inCP;
        uint outCP;

        if (!GetConsoleMode(TerminalIn.Handle, out var inMode) ||
            !GetConsoleMode(TerminalOut.Handle, out var outMode) ||
            (inCP = GetConsoleCP()) == 0 ||
            (outCP = GetConsoleOutputCP()) == 0)
            throw new TerminalNotAttachedException();

        // Stash away the original modes the first time we are successfully called.
        if (_original == null)
            _original = new(inMode, outMode, inCP, outCP);

        var origIn = inMode;
        var origOut = outMode;

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

        try
        {
            var utf8 = (uint)Encoding.UTF8.CodePage;

            if (!SetConsoleCP(utf8) ||
                !SetConsoleOutputCP(utf8))
                throw new TerminalConfigurationException(
                    $"Could not change console code page: {new Win32Exception().Message}");

            if (!SetConsoleMode(TerminalIn.Handle, inMode) ||
                !SetConsoleMode(TerminalOut.Handle, outMode))
                throw new TerminalConfigurationException(
                    $"Could not change console mode: {new Win32Exception().Message}");

            if (flush && !FlushConsoleInputBuffer(TerminalIn.Handle))
                throw new TerminalConfigurationException(
                    $"Could not flush input buffer: {new Win32Exception().Message}");
        }
        catch (TerminalConfigurationException)
        {
            // If we failed to configure the console, try to undo partial configuration (if any).

            _ = SetConsoleMode(TerminalIn.Handle, origIn);
            _ = SetConsoleMode(TerminalOut.Handle, origOut);

            _ = SetConsoleCP(inCP);
            _ = SetConsoleOutputCP(outCP);

            throw;
        }
    }

    protected override void SetMode(bool raw)
    {
        SetModeCore(raw, true);
    }

    public override void DangerousRestoreSettings()
    {
        using var guard = Control.Guard();

        if (_original != null)
        {
            _ = SetConsoleMode(TerminalIn.Handle, _original.InMode);
            _ = SetConsoleMode(TerminalOut.Handle, _original.OutMode);

            _ = SetConsoleCP(_original.InCodePage);
            _ = SetConsoleOutputCP(_original.OutCodePage);
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
