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

        readonly byte[] _buffer;

        ReadOnlyMemory<byte> _buffered;

        public WindowsTerminalReader(TerminalDriver driver, SafeHandle handle, string name)
            : base(driver)
        {
            Handle = handle;
            IsValid = IsHandleValid(handle, false);
            _name = name;
            _buffer = new byte[Encoding.GetMaxByteCount(2)];
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

            // The Windows console host is eventually going to support UTF-8 input via the ReadFile function. Sadly,
            // this does not work today; non-ASCII characters just turn into NULs. This means that we have to use the
            // ReadConsoleW function for interactive input and ReadFile for redirected input. This complicates the
            // interactive case considerably since ReadConsoleW operates in terms of UTF-16 code units while the API we
            // offer operates in terms of raw bytes.
            //
            // To solve this problem, we read one or two UTF-16 code units to form a complete code point. We then encode
            // that into UTF-8 in a separate buffer. Finally, we copy as many bytes as possible/requested from the UTF-8
            // buffer to the caller-provided buffer.
            if (!IsRedirected)
            {
                lock (_lock)
                {
                    if (_buffered.IsEmpty)
                    {
                        Span<char> units = stackalloc char[2];
                        var count = 0;

                        fixed (char* p = units)
                        {
                            bool ret;
                            uint read = 0;

                            while ((ret = PInvoke.ReadConsoleW(Handle, p, 1, out read, null)) &&
                                Marshal.GetLastSystemError() == (int)WIN32_ERROR.ERROR_OPERATION_ABORTED)
                            {
                                // Retry in case we get interrupted by a signal.
                            }

                            if (!ret)
                                return HandleError(read, $"Could not read from standard {_name}");

                            if (read == 0)
                                return 0;

                            // There is a bug where ReadConsoleW will not process Ctrl-Z properly even though ReadFile
                            // will. The good news is that we can fairly easily emulate what the console host should be
                            // doing by just pretending there is no more data to be read.
                            if (!Driver.IsRawMode && units[0] == '\x1a')
                                return 0;

                            count++;

                            // If we got a high surrogate, we expect to instantly see a low surrogate following it. In
                            // really bizarre situations (e.g. broken WriteConsoleInput calls), this might not be the
                            // case, though; in such a case, we will just let UTF8Encoding encode the lone high
                            // surrogate into a replacement character (U+FFFD).
                            //
                            // It is not really clear whether this is the right thing to do. A case could easily be made
                            // for passing the lone surrogate through unmodified or simply discarding it...
                            if (char.IsHighSurrogate(units[0]))
                            {
                                while ((ret = PInvoke.ReadConsoleW(Handle, p + 1, 1, out read, null)) &&
                                    Marshal.GetLastSystemError() == (int)WIN32_ERROR.ERROR_OPERATION_ABORTED)
                                {
                                    // Retry in case we get interrupted by a signal.
                                }

                                if (!ret)
                                    return HandleError(read, $"Could not read from standard {_name}");

                                if (read != 0)
                                    count++;
                            }

                            // Encode the UTF-16 code unit(s) into UTF-8 and grab a slice of the buffer corresponding to
                            // just the portion used.
                            _buffered = _buffer.AsMemory(0, Encoding.GetBytes(units[0..count], _buffer));
                        }
                    }

                    // Now that we have some UTF-8 text buffered up, we can copy it over to the buffer provided by the
                    // caller and adjust our UTF-8 buffer accordingly. Be careful not to overrun either buffer.
                    var copied = Math.Min(_buffered.Length, data.Length);

                    _buffered.Span[0..copied].CopyTo(data[0..copied]);
                    _buffered = _buffered[copied..];

                    return copied;
                }
            }
            else
            {
                uint ret;

                lock (_lock)
                    fixed (byte* p = data)
                        if (PInvoke.ReadFile(Handle, p, (uint)data.Length, &ret, null))
                            return (int)ret;

                return HandleError(ret, $"Could not read from standard {_name}");
            }
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

            _ = HandleError(0, $"Could not write to standard {_name}");
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

        // Input needs to be UTF-16, but we make it appear as if it is UTF-8 to users of the library. See the comments
        // in WindowsTerminalReader for the gory details.
        _ = PInvoke.SetConsoleCP((uint)Encoding.Unicode.CodePage);
        _ = PInvoke.SetConsoleOutputCP((uint)Encoding.UTF8.CodePage);

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

        // Set modes on all handles in case one of them has been redirected. These calls can fail if there is no console
        // attached, but that is OK.
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

        // We need to grab the window size at least once upfront so that adding an event handler to the Resize event
        // does not immediately cause the event to be fired.
        RefreshWindowSize();

        // Windows currently has no SIGWINCH equivalent, so we have to poll for size changes.
        _ = TerminalUtility.StartThread("Terminal Resize Listener", () =>
        {
            while (true)
            {
                _event.Wait();

                // HandleResize will check that the size is actually different from the last time the event was fired.
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

    static int HandleError(uint result, string message)
    {
        // TODO: https://github.com/microsoft/CsWin32/issues/452
        var err = (WIN32_ERROR)Marshal.GetLastSystemError();

        // See comments in UnixTerminalWriter for the error handling rationale.
        return err switch
        {
            WIN32_ERROR.ERROR_HANDLE_EOF or WIN32_ERROR.ERROR_BROKEN_PIPE or WIN32_ERROR.ERROR_NO_DATA => (int)result,
            _ => throw new TerminalException($"{message}: {err}"),
        };
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
                $"Could not change raw mode setting: {(WIN32_ERROR)Marshal.GetLastSystemError()}");

        if (!PInvoke.FlushConsoleInputBuffer(InHandle))
            throw new TerminalException(
                $"Could not flush input buffer: {(WIN32_ERROR)Marshal.GetLastSystemError()}");
    }
}
