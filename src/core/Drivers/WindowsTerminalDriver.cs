using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Console;

namespace System.Drivers;

sealed class WindowsTerminalDriver : TerminalDriver
{
    sealed class WindowsTerminalReader : TerminalReader
    {
        public SafeHandle Handle { get; }

        public bool IsValid { get; }

        public override bool IsRedirected => IsRedirected(Handle);

        readonly object _lock = new();

        readonly string _name;

        public WindowsTerminalReader(TerminalDriver driver, SafeHandle handle, string name)
            : base(driver)
        {
            Handle = handle;
            IsValid = IsHandleValid(handle, false);
            _name = name;
        }

        public CONSOLE_MODE? GetMode()
        {
            return PInvoke.GetConsoleMode(Handle, out var m) ? m : null;
        }

        public bool SetMode(CONSOLE_MODE mode)
        {
            return PInvoke.SetConsoleMode(Handle, mode);
        }

        public bool AddMode(CONSOLE_MODE mode)
        {
            return GetMode() is CONSOLE_MODE m && PInvoke.SetConsoleMode(Handle, m | mode);
        }

        public bool RemoveMode(CONSOLE_MODE mode)
        {
            return GetMode() is CONSOLE_MODE m && PInvoke.SetConsoleMode(Handle, m & ~mode);
        }

        public override unsafe int Read(Span<byte> data)
        {
            if (data.IsEmpty || !IsValid)
                return 0;

            uint ret;

            lock (_lock)
                fixed (byte* p = data)
                    if (PInvoke.ReadFile(Handle, p, (uint)data.Length, &ret, null))
                        return (int)ret;

            var err = (WIN32_ERROR)Marshal.GetLastPInvokeError();

            // See comments in UnixTerminalReader for the error handling rationale.
            switch (err)
            {
                case WIN32_ERROR.ERROR_HANDLE_EOF:
                case WIN32_ERROR.ERROR_BROKEN_PIPE:
                case WIN32_ERROR.ERROR_NO_DATA:
                    break;
                default:
                    throw new TerminalException($"Could not read from standard {_name}: {err}");
            }

            return (int)ret;
        }
    }

    sealed class WindowsTerminalWriter : TerminalWriter
    {
        public SafeHandle Handle { get; }

        public bool IsValid { get; }

        public override bool IsRedirected => IsRedirected(Handle);

        readonly object _lock = new();

        readonly string _name;

        public WindowsTerminalWriter(TerminalDriver driver, SafeHandle handle, string name)
            : base(driver)
        {
            Handle = handle;
            IsValid = IsHandleValid(handle, true);
            _name = name;
        }

        public CONSOLE_MODE? GetMode()
        {
            return PInvoke.GetConsoleMode(Handle, out var m) ? m : null;
        }

        public bool SetMode(CONSOLE_MODE mode)
        {
            return PInvoke.SetConsoleMode(Handle, mode);
        }

        public bool AddMode(CONSOLE_MODE mode)
        {
            return GetMode() is CONSOLE_MODE m && PInvoke.SetConsoleMode(Handle, m | mode);
        }

        public bool RemoveMode(CONSOLE_MODE mode)
        {
            return GetMode() is CONSOLE_MODE m && PInvoke.SetConsoleMode(Handle, m & ~mode);
        }

        public override unsafe void Write(ReadOnlySpan<byte> data)
        {
            if (data.IsEmpty || !IsValid)
                return;

            lock (_lock)
            {
                fixed (byte* p = data)
                {
                    uint dummy;

                    if (PInvoke.WriteFile(Handle, p, (uint)data.Length, &dummy, null))
                        return;
                }
            }

            var err = (WIN32_ERROR)Marshal.GetLastPInvokeError();

            // See comments in UnixTerminalWriter for the error handling rationale.
            switch (err)
            {
                case WIN32_ERROR.ERROR_HANDLE_EOF:
                case WIN32_ERROR.ERROR_BROKEN_PIPE:
                case WIN32_ERROR.ERROR_NO_DATA:
                    break;
                default:
                    throw new TerminalException($"Could not write to standard {_name}: {err}");
            }
        }
    }

    public static WindowsTerminalDriver Instance { get; } = new();

    static SafeHandle InHandle => PInvoke.GetStdHandle_SafeHandle(STD_HANDLE.STD_INPUT_HANDLE);

    static SafeHandle OutHandle => PInvoke.GetStdHandle_SafeHandle(STD_HANDLE.STD_OUTPUT_HANDLE);

    static SafeHandle ErrorHandle => PInvoke.GetStdHandle_SafeHandle(STD_HANDLE.STD_ERROR_HANDLE);

    public override TerminalReader StdIn => _in;

    public override TerminalWriter StdOut => _out;

    public override TerminalWriter StdError => _error;

    public override TerminalSize Size
    {
        get
        {
            if (GetSize() is TerminalSize s)
                _size = s;

            return _size ?? throw new TerminalException("There is no terminal attached.");
        }
    }

    readonly ManualResetEventSlim _event = new();

    readonly WindowsTerminalReader _in;

    readonly WindowsTerminalWriter _out;

    readonly WindowsTerminalWriter _error;

    TerminalSize? _size;

    WindowsTerminalDriver()
    {
        _in = new(this, InHandle, "input");
        _out = new(this, OutHandle, "output");
        _error = new(this, ErrorHandle, "error");

        _ = PInvoke.SetConsoleCP((uint)Encoding.CodePage);
        _ = PInvoke.SetConsoleOutputCP((uint)Encoding.CodePage);

        var inMode =
            CONSOLE_MODE.ENABLE_PROCESSED_INPUT |
            CONSOLE_MODE.ENABLE_LINE_INPUT |
            CONSOLE_MODE.ENABLE_ECHO_INPUT |
            CONSOLE_MODE.ENABLE_INSERT_MODE |
            CONSOLE_MODE.ENABLE_EXTENDED_FLAGS |
            CONSOLE_MODE.ENABLE_VIRTUAL_TERMINAL_INPUT;
        var outMode =
            CONSOLE_MODE.ENABLE_PROCESSED_OUTPUT |
            CONSOLE_MODE.ENABLE_WRAP_AT_EOL_OUTPUT |
            CONSOLE_MODE.ENABLE_VIRTUAL_TERMINAL_PROCESSING;

        // Set modes on all handles in case one of them has been redirected. These calls can
        // fail if there is no console attached, but that is OK.
        _ = _in.AddMode(inMode);
        _ = _out.AddMode(outMode) || _error.AddMode(outMode);

        void RefreshWindowSize()
        {
            if (GetSize() is TerminalSize s)
            {
                _size = s;

                HandleResize(s);
            }
        }

        // We need to grab the window size at least once upfront so that adding an event handler
        // to the Resize event does not immediately cause the event to be fired.
        RefreshWindowSize();

        // Windows currently has no SIGWINCH equivalent, so we have to poll for size changes.
        _ = TerminalUtility.StartThread("Terminal Resize Listener", () =>
        {
            while (true)
            {
                _event.Wait();

                // HandleResize will check whether the size is actually different from the last
                // time the event was fired.
                RefreshWindowSize();

                // TODO: Do we need to make this configurable?
                Thread.Sleep(100);
            }
        });
    }

    protected override void ToggleResizeEvent(bool enable)
    {
        if (enable)
            _event.Set();
        else
            _event.Reset();
    }

    public override void GenerateBreakSignal(TerminalBreakSignal signal)
    {
        _ = PInvoke.GenerateConsoleCtrlEvent(
            signal switch
            {
                TerminalBreakSignal.Interrupt => Constants.CTRL_C_EVENT,
                TerminalBreakSignal.Quit => Constants.CTRL_BREAK_EVENT,
                _ => throw new ArgumentOutOfRangeException(nameof(signal)),
            },
            0);
    }

    public override void GenerateSuspendSignal()
    {
        // Windows does not have an equivalent of SIGTSTP.
    }

    static unsafe bool IsHandleValid(SafeHandle handle, bool write)
    {
        if (handle.IsInvalid)
            return false;

        if (write)
        {
            uint dummy = 42;

            return PInvoke.WriteFile(handle, &dummy, 0, &dummy, null);
        }

        return true;
    }

    static bool IsRedirected(SafeHandle handle)
    {
        return PInvoke.GetFileType(handle) != Constants.FILE_TYPE_CHAR || !PInvoke.GetConsoleMode(handle, out _);
    }

    TerminalSize? GetSize()
    {
        static CONSOLE_SCREEN_BUFFER_INFO? GetInfo(SafeHandle handle)
        {
            return PInvoke.GetConsoleScreenBufferInfo(handle, out var info) ? info : null;
        }

        // Try both handles in case only one of them has been redirected.
        return (GetInfo(_out.Handle) ?? GetInfo(_error.Handle)) is CONSOLE_SCREEN_BUFFER_INFO i ?
            new(i.srWindow.Right - i.srWindow.Left + 1, i.srWindow.Bottom - i.srWindow.Top + 1) : null;
    }

    protected override void SetRawModeCore(bool raw, bool discard)
    {
        if (!_in.IsValid || (!_out.IsValid && !_error.IsValid))
            throw new TerminalException("There is no terminal attached.");

        var inMode =
            CONSOLE_MODE.ENABLE_PROCESSED_INPUT |
            CONSOLE_MODE.ENABLE_LINE_INPUT |
            CONSOLE_MODE.ENABLE_ECHO_INPUT;
        var outMode =
            CONSOLE_MODE.DISABLE_NEWLINE_AUTO_RETURN;

        if (!(raw ? _in.RemoveMode(inMode) && (_out.RemoveMode(outMode) || _error.RemoveMode(outMode)) :
            _in.AddMode(inMode) && (_out.AddMode(outMode) || _error.AddMode(outMode))))
            throw new TerminalException(
                $"Could not change raw mode setting: {(WIN32_ERROR)Marshal.GetLastPInvokeError()}");

        if (!PInvoke.FlushConsoleInputBuffer(InHandle))
            throw new TerminalException(
                $"Could not flush input buffer: {(WIN32_ERROR)Marshal.GetLastPInvokeError()}");
    }
}
