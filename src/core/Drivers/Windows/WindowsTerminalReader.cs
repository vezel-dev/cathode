using Windows.Win32.Foundation;
using Windows.Win32.System.Console;
using static Windows.Win32.WindowsPInvoke;

namespace System.Drivers.Windows;

sealed class WindowsTerminalReader : DefaultTerminalReader
{
    public SafeHandle Handle { get; }

    public bool IsValid { get; }

    public override bool IsRedirected { get; }

    readonly object _lock = new();

    readonly WindowsTerminalDriver _driver;

    readonly byte[] _buffer;

    ReadOnlyMemory<byte> _buffered;

    public WindowsTerminalReader(WindowsTerminalDriver driver, SafeHandle handle)
    {
        Handle = handle;
        IsValid = WindowsTerminalUtility.IsHandleValid(handle, false);
        IsRedirected = WindowsTerminalUtility.IsRedirected(handle);
        _driver = driver;
        _buffer = new byte[Terminal.Encoding.GetMaxByteCount(2)];
    }

    protected override unsafe int ReadCore(Span<byte> data)
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

                        while ((ret = ReadConsoleW(Handle, p, 1, out read, null)) &&
                            Marshal.GetLastSystemError() == (int)WIN32_ERROR.ERROR_OPERATION_ABORTED)
                        {
                            // Retry in case we get interrupted by a signal.
                        }

                        if (!ret)
                            return WindowsTerminalUtility.HandleError(read, $"Could not read from standard input");

                        if (read == 0)
                            return 0;

                        // There is a bug where ReadConsoleW will not process Ctrl-Z properly even though ReadFile
                        // will. The good news is that we can fairly easily emulate what the console host should be
                        // doing by just pretending there is no more data to be read.
                        //
                        // TODO: Review this for race conditions with changing raw mode.
                        if (!_driver.IsRawMode && units[0] == '\x1a')
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
                            while ((ret = ReadConsoleW(Handle, p + 1, 1, out read, null)) &&
                                Marshal.GetLastSystemError() == (int)WIN32_ERROR.ERROR_OPERATION_ABORTED)
                            {
                                // Retry in case we get interrupted by a signal.
                            }

                            if (!ret)
                                return WindowsTerminalUtility.HandleError(read, $"Could not read from standard input");

                            if (read != 0)
                                count++;
                        }

                        // Encode the UTF-16 code unit(s) into UTF-8 and grab a slice of the buffer corresponding to
                        // just the portion used.
                        _buffered = _buffer.AsMemory(0, Terminal.Encoding.GetBytes(units[0..count], _buffer));
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
                    if (ReadFile(Handle, p, (uint)data.Length, &ret, null))
                        return (int)ret;

            return WindowsTerminalUtility.HandleError(ret, $"Could not read from standard input");
        }
    }

    public CONSOLE_MODE? GetMode()
    {
        return GetConsoleMode(Handle, out var m) ? m : null;
    }

    public bool SetMode(CONSOLE_MODE mode)
    {
        return SetConsoleMode(Handle, mode);
    }

    public bool AddMode(CONSOLE_MODE mode)
    {
        return GetMode() is CONSOLE_MODE m && SetConsoleMode(Handle, m | mode);
    }

    public bool RemoveMode(CONSOLE_MODE mode)
    {
        return GetMode() is CONSOLE_MODE m && SetConsoleMode(Handle, m & ~mode);
    }
}
