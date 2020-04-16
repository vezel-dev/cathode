using System.IO;
using System.Threading;
using Vanara.PInvoke;

namespace System.Drivers
{
#pragma warning disable CA1001

    sealed class WindowsTerminalDriver : TerminalDriver
    {
        sealed class WindowsTerminalReader : TerminalReader
        {
            public HFILE Handle { get; }

            public bool IsValid { get; }

            public override bool IsRedirected => IsRedirected(Handle);

            readonly object _lock = new object();

            readonly string _name;

            public WindowsTerminalReader(TerminalDriver driver, HFILE handle, string name)
                : base(driver)
            {
                Handle = handle;
                IsValid = IsHandleValid(handle, false);
                _name = name;
            }

            public Kernel32.CONSOLE_INPUT_MODE? GetMode()
            {
                return Kernel32.GetConsoleMode(Handle, out Kernel32.CONSOLE_INPUT_MODE m) ? m : default;
            }

            public bool SetMode(Kernel32.CONSOLE_INPUT_MODE mode)
            {
                return Kernel32.SetConsoleMode(Handle, mode);
            }

            public bool AddMode(Kernel32.CONSOLE_INPUT_MODE mode)
            {
                return GetMode() is Kernel32.CONSOLE_INPUT_MODE m && Kernel32.SetConsoleMode(Handle, m | mode);
            }

            public bool RemoveMode(Kernel32.CONSOLE_INPUT_MODE mode)
            {
                return GetMode() is Kernel32.CONSOLE_INPUT_MODE m && Kernel32.SetConsoleMode(Handle, m & ~mode);
            }

            public override unsafe int Read(Span<byte> data)
            {
                if (data.IsEmpty || !IsValid)
                    return 0;

                uint ret;

                lock (_lock)
                    fixed (byte* p = data)
                        if (Kernel32.ReadFile(Handle, (IntPtr)p, (uint)data.Length, out ret, IntPtr.Zero))
                            return (int)ret;

                var err = Win32Error.GetLastError();

                // See comments in UnixTerminalReader for the error handling rationale.
                switch ((int)err)
                {
                    case Win32Error.ERROR_HANDLE_EOF:
                    case Win32Error.ERROR_BROKEN_PIPE:
                    case Win32Error.ERROR_NO_DATA:
                        break;
                    default:
                        throw new TerminalException($"Could not read from standard {_name}: {err.FormatMessage()}");
                }

                return (int)ret;
            }
        }

        sealed class WindowsTerminalWriter : TerminalWriter
        {
            public HFILE Handle { get; }

            public bool IsValid { get; }

            public override bool IsRedirected => IsRedirected(Handle);

            readonly object _lock = new object();

            readonly string _name;

            public WindowsTerminalWriter(TerminalDriver driver, HFILE handle, string name)
                : base(driver)
            {
                Handle = handle;
                IsValid = IsHandleValid(handle, true);
                _name = name;
            }

            public Kernel32.CONSOLE_OUTPUT_MODE? GetMode()
            {
                return Kernel32.GetConsoleMode(Handle, out Kernel32.CONSOLE_OUTPUT_MODE m) ? m : default;
            }

            public bool SetMode(Kernel32.CONSOLE_OUTPUT_MODE mode)
            {
                return Kernel32.SetConsoleMode(Handle, mode);
            }

            public bool AddMode(Kernel32.CONSOLE_OUTPUT_MODE mode)
            {
                return GetMode() is Kernel32.CONSOLE_OUTPUT_MODE m && Kernel32.SetConsoleMode(Handle, m | mode);
            }

            public bool RemoveMode(Kernel32.CONSOLE_OUTPUT_MODE mode)
            {
                return GetMode() is Kernel32.CONSOLE_OUTPUT_MODE m && Kernel32.SetConsoleMode(Handle, m & ~mode);
            }

            public override unsafe void Write(ReadOnlySpan<byte> data)
            {
                if (data.IsEmpty || !IsValid)
                    return;

                lock (_lock)
                    fixed (byte* p = data)
                        if (Kernel32.WriteFile(Handle, (IntPtr)p, (uint)data.Length, out _, IntPtr.Zero))
                            return;

                var err = Win32Error.GetLastError();

                // See comments in UnixTerminalWriter for the error handling rationale.
                switch ((int)err)
                {
                    case Win32Error.ERROR_HANDLE_EOF:
                    case Win32Error.ERROR_BROKEN_PIPE:
                    case Win32Error.ERROR_NO_DATA:
                        break;
                    default:
                        throw new TerminalException($"Could not write to standard {_name}: {err.FormatMessage()}");
                }
            }
        }

        public static WindowsTerminalDriver Instance { get; } = new WindowsTerminalDriver();

        static HFILE InHandle => Kernel32.GetStdHandle(Kernel32.StdHandleType.STD_INPUT_HANDLE);

        static HFILE OutHandle => Kernel32.GetStdHandle(Kernel32.StdHandleType.STD_OUTPUT_HANDLE);

        static HFILE ErrorHandle => Kernel32.GetStdHandle(Kernel32.StdHandleType.STD_ERROR_HANDLE);

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

        readonly ManualResetEventSlim _event = new ManualResetEventSlim();

        readonly WindowsTerminalReader _in;

        readonly WindowsTerminalWriter _out;

        readonly WindowsTerminalWriter _error;

#pragma warning disable IDE0052

        readonly Kernel32.HandlerRoutine _handler;

#pragma warning restore IDE0052

        TerminalSize? _size;

        WindowsTerminalDriver()
        {
            _in = new WindowsTerminalReader(this, InHandle, "input");
            _out = new WindowsTerminalWriter(this, OutHandle, "output");
            _error = new WindowsTerminalWriter(this, ErrorHandle, "error");

            _ = Kernel32.SetConsoleCP((uint)Encoding.CodePage);
            _ = Kernel32.SetConsoleOutputCP((uint)Encoding.CodePage);

            var inMode =
                Kernel32.CONSOLE_INPUT_MODE.ENABLE_PROCESSED_INPUT |
                Kernel32.CONSOLE_INPUT_MODE.ENABLE_LINE_INPUT |
                Kernel32.CONSOLE_INPUT_MODE.ENABLE_ECHO_INPUT |
                Kernel32.CONSOLE_INPUT_MODE.ENABLE_INSERT_MODE |
                Kernel32.CONSOLE_INPUT_MODE.ENABLE_EXTENDED_FLAGS |
                Kernel32.CONSOLE_INPUT_MODE.ENABLE_VIRTUAL_TERMINAL_INPUT;
            var outMode =
                Kernel32.CONSOLE_OUTPUT_MODE.ENABLE_PROCESSED_OUTPUT |
                Kernel32.CONSOLE_OUTPUT_MODE.ENABLE_WRAP_AT_EOL_OUTPUT |
                Kernel32.CONSOLE_OUTPUT_MODE.ENABLE_VIRTUAL_TERMINAL_PROCESSING;

            // Set modes on all handles in case one of them has been redirected. These calls can
            // fail if there is no console attached, but that is OK.
            _ = _in.AddMode(inMode);
            _ = _out.AddMode(outMode) || _error.AddMode(outMode);

            // Keep the delegate alive by storing it in a field.
            _handler = e => HandleBreakSignal(e == Kernel32.CTRL_EVENT.CTRL_C_EVENT);

            _ = Kernel32.SetConsoleCtrlHandler(_handler, true);

            // Windows currently has no SIGWINCH equivalent, so we have to poll for size changes.
            _ = TerminalUtility.StartThread("Terminal Resize Listener", () =>
            {
                while (true)
                {
                    _event.Wait();

                    // HandleResize will check whether the size is actually different from the last
                    // time the event was fired.
                    if (GetSize() is TerminalSize s)
                    {
                        _size = s;

                        HandleResize(s);
                    }

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
            _ = Kernel32.GenerateConsoleCtrlEvent(signal switch
            {
                TerminalBreakSignal.Interrupt => Kernel32.CTRL_EVENT.CTRL_C_EVENT,
                TerminalBreakSignal.Quit => Kernel32.CTRL_EVENT.CTRL_BREAK_EVENT,
                _ => throw new ArgumentOutOfRangeException(nameof(signal)),
            }, 0);
        }

        public override void GenerateSuspendSignal()
        {
            // Windows does not have an equivalent of SIGTSTP.
        }

        static unsafe bool IsHandleValid(HFILE handle, bool write)
        {
            if (handle.IsNull || handle.IsInvalid)
                return false;

            if (write)
            {
                byte dummy = 42;

                return Kernel32.WriteFile(handle, (IntPtr)(&dummy), 0, out _, IntPtr.Zero);
            }

            return true;
        }

        static bool IsRedirected(HFILE handle)
        {
            return !Kernel32.GetFileType(handle).HasFlag(Kernel32.FileType.FILE_TYPE_CHAR) ||
                !Kernel32.GetConsoleMode(handle, out Kernel32.CONSOLE_INPUT_MODE _);
        }

        TerminalSize? GetSize()
        {
            static Kernel32.CONSOLE_SCREEN_BUFFER_INFO? GetInfo(HFILE handle)
            {
                return Kernel32.GetConsoleScreenBufferInfo(handle, out var info) ? info : default;
            }

            // Try both handles in case only one of them has been redirected.
            return (GetInfo(_out.Handle) ?? GetInfo(_error.Handle)) is Kernel32.CONSOLE_SCREEN_BUFFER_INFO i ?
                new TerminalSize(i.srWindow.Right - i.srWindow.Left + 1, i.srWindow.Bottom - i.srWindow.Top + 1) :
                default;
        }

        protected override void SetRawModeCore(bool raw, bool discard)
        {
            if (!_in.IsValid || (!_out.IsValid && !_error.IsValid))
                throw new TerminalException("There is no terminal attached.");

            var inMode =
                Kernel32.CONSOLE_INPUT_MODE.ENABLE_PROCESSED_INPUT |
                Kernel32.CONSOLE_INPUT_MODE.ENABLE_LINE_INPUT |
                Kernel32.CONSOLE_INPUT_MODE.ENABLE_ECHO_INPUT;
            var outMode =
                Kernel32.CONSOLE_OUTPUT_MODE.DISABLE_NEWLINE_AUTO_RETURN;

            if (!(raw ? _in.RemoveMode(inMode) && (_out.RemoveMode(outMode) || _error.RemoveMode(outMode)) :
                _in.AddMode(inMode) && (_out.AddMode(outMode) || _error.AddMode(outMode))))
                throw new TerminalException(
                    $"Could not change raw mode setting: {Win32Error.GetLastError().FormatMessage()}");

            if (!Kernel32.FlushConsoleInputBuffer(InHandle))
                throw new TerminalException(
                    $"Could not flush input buffer: {Win32Error.GetLastError().FormatMessage()}");
        }
    }

#pragma warning restore CA1001
}
