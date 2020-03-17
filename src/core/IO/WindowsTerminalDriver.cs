using System.Text;
using Vanara.PInvoke;

namespace System.IO
{
    sealed class WindowsTerminalDriver : ITerminalDriver
    {
        abstract class WindowsTerminalHandle : ITerminalHandle
        {
            public HFILE Handle { get; }

            public Encoding Encoding { get; }

            public virtual bool IsRedirected => !Kernel32.GetFileType(Handle).HasFlag(Kernel32.FileType.FILE_TYPE_CHAR);

            protected string Name { get; }

            protected object Lock { get; } = new object();

            protected WindowsTerminalHandle(HFILE handle, Encoding encoding, string name)
            {
                Handle = handle;
                Encoding = encoding;
                Name = name;
            }

            protected static unsafe bool IsHandleValid(HFILE handle, bool write)
            {
                if (handle.IsNull || handle.IsInvalid)
                    return false;

                if (write)
                {
                    byte dummy = 0;

                    return Kernel32.WriteFile(handle, (IntPtr)(&dummy), 0, out _, IntPtr.Zero);
                }

                return true;
            }

            public Kernel32.CONSOLE_SCREEN_BUFFER_INFO? GetBufferInfo()
            {
                return Kernel32.GetConsoleScreenBufferInfo(Handle, out var info) ? info : default;
            }
        }

        sealed class WindowsTerminalReader : WindowsTerminalHandle, ITerminalReader
        {
            public TerminalInputStream Stream { get; }

            public bool IsValid { get; }

            public override bool IsRedirected => base.IsRedirected || GetMode() == null;

            public WindowsTerminalReader(HFILE handle, Encoding encoding, string name)
                : base(handle, encoding, name)
            {
                Stream = new TerminalInputStream(this);
                IsValid = IsHandleValid(handle, false);
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

            public unsafe int Read(Span<byte> data)
            {
                if (data.IsEmpty || !IsValid)
                    return 0;

                uint ret;

                lock (Lock)
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
                        throw new TerminalException($"Could not read from standard {Name}: {err.FormatMessage()}");
                }

                return (int)ret;
            }
        }

        sealed class WindowsTerminalWriter : WindowsTerminalHandle, ITerminalWriter
        {
            public TerminalOutputStream Stream { get; }

            public bool IsValid { get; }

            public override bool IsRedirected => base.IsRedirected || GetMode() == null;

            public WindowsTerminalWriter(HFILE handle, Encoding encoding, string name)
                : base(handle, encoding, name)
            {
                Stream = new TerminalOutputStream(this);
                IsValid = IsHandleValid(handle, true);
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

            public unsafe void Write(ReadOnlySpan<byte> data)
            {
                if (data.IsEmpty || !IsValid)
                    return;

                lock (Lock)
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
                        throw new TerminalException($"Could not write to standard {Name}: {err.FormatMessage()}");
                }
            }
        }

        public static WindowsTerminalDriver Instance { get; } = new WindowsTerminalDriver();

        static HFILE InHandle => Kernel32.GetStdHandle(Kernel32.StdHandleType.STD_INPUT_HANDLE);

        static HFILE OutHandle => Kernel32.GetStdHandle(Kernel32.StdHandleType.STD_OUTPUT_HANDLE);

        static HFILE ErrorHandle => Kernel32.GetStdHandle(Kernel32.StdHandleType.STD_ERROR_HANDLE);

        public ITerminalReader StdIn => _in;

        public ITerminalWriter StdOut => _out;

        public ITerminalWriter StdError => _error;

        public int Width =>
            GetBufferInfo() is Kernel32.CONSOLE_SCREEN_BUFFER_INFO i ?
                (_width = i.srWindow.Right - i.srWindow.Left + 1) : _width;

        public int Height =>
            GetBufferInfo() is Kernel32.CONSOLE_SCREEN_BUFFER_INFO i ?
                (_height = i.srWindow.Bottom - i.srWindow.Top + 1) : _height;

        static readonly Encoding _encoding = Encoding.UTF8;

        readonly WindowsTerminalReader _in = new WindowsTerminalReader(InHandle, _encoding, "input");

        readonly WindowsTerminalWriter _out = new WindowsTerminalWriter(OutHandle, _encoding, "output");

        readonly WindowsTerminalWriter _error = new WindowsTerminalWriter(ErrorHandle, _encoding, "error");

        int _width = TerminalUtility.InvalidSize;

        int _height = TerminalUtility.InvalidSize;

        WindowsTerminalDriver()
        {
            _ = Kernel32.SetConsoleCP((uint)_encoding.CodePage);
            _ = Kernel32.SetConsoleOutputCP((uint)_encoding.CodePage);

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
                Kernel32.CONSOLE_OUTPUT_MODE.ENABLE_VIRTUAL_TERMINAL_PROCESSING |
                Kernel32.CONSOLE_OUTPUT_MODE.DISABLE_NEWLINE_AUTO_RETURN;

            // Set modes on all handles in case one of them has been redirected. These calls can
            // fail if there is no console attached, but that is OK.
            _ = _in.AddMode(inMode);
            _ = _out.AddMode(outMode) || _error.AddMode(outMode);
        }

        Kernel32.CONSOLE_SCREEN_BUFFER_INFO? GetBufferInfo()
        {
            // Try both handles in case only one of them has been redirected.
            return _out.GetBufferInfo() ?? _error.GetBufferInfo();
        }

        public void SetRawMode(bool raw, bool discard)
        {
            if (!_in.IsValid || (!_out.IsValid && !_error.IsValid))
                throw new TerminalException("There is no terminal attached.");

            var inMode =
                Kernel32.CONSOLE_INPUT_MODE.ENABLE_PROCESSED_INPUT |
                Kernel32.CONSOLE_INPUT_MODE.ENABLE_LINE_INPUT |
                Kernel32.CONSOLE_INPUT_MODE.ENABLE_ECHO_INPUT;
            var outMode =
                Kernel32.CONSOLE_OUTPUT_MODE.ENABLE_PROCESSED_OUTPUT |
                Kernel32.CONSOLE_OUTPUT_MODE.ENABLE_WRAP_AT_EOL_OUTPUT;

            // TODO: Respect discard somehow?
            if (!(raw ? _in.RemoveMode(inMode) && (_out.RemoveMode(outMode) || _error.RemoveMode(outMode)) :
                _in.AddMode(inMode) && (_out.AddMode(outMode) || _error.AddMode(outMode))))
                throw new TerminalException(
                    $"Could not switch to raw mode: {Win32Error.GetLastError().FormatMessage()}");
        }
    }
}
