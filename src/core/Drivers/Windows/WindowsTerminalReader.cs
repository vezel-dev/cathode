using Windows.Win32.Foundation;
using Windows.Win32.System.Console;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using static Windows.Win32.WindowsPInvoke;

namespace System.Drivers.Windows;

sealed class WindowsTerminalReader : DriverTerminalReader<WindowsTerminalDriver, SafeHandle>
{
    readonly object _lock;

    readonly WindowsCancellationEvent _cancellationEvent;

    readonly byte[] _buffer;

    ReadOnlyMemory<byte> _buffered;

    public WindowsTerminalReader(
        WindowsTerminalDriver driver,
        string name,
        SafeHandle handle,
        WindowsCancellationEvent cancellationEvent,
        object @lock)
        : base(driver, name, handle)
    {
        _lock = @lock;
        _cancellationEvent = cancellationEvent;
        _buffer = new byte[Terminal.Encoding.GetMaxByteCount(2)];
    }

    protected override unsafe int ReadBufferCore(Span<byte> buffer, CancellationToken cancellationToken)
    {
        // If the handle is invalid, just present the illusion to the user that it has been redirected to /dev/null or
        // something along those lines, i.e. return EOF.
        if (buffer.IsEmpty || !IsValid)
            return 0;

        // The Windows console host is eventually going to support UTF-8 input via the ReadFile function. Sadly, this
        // does not work today; non-ASCII characters just turn into NULs. This means that we have to use the
        // ReadConsoleW function for interactive input and ReadFile for redirected input. This complicates the
        // interactive case considerably since ReadConsoleW operates in terms of UTF-16 code units while the API we
        // offer operates in terms of raw bytes.
        //
        // To solve this problem, we read one or two UTF-16 code units to form a complete code point. We then encode
        // that into UTF-8 in a separate buffer. Finally, we copy as many bytes as possible/requested from the UTF-8
        // buffer to the caller-provided buffer.
        if (IsInteractive)
        {
            lock (_lock)
            {
                if (_buffered.IsEmpty)
                {
                    _cancellationEvent.PollWithCancellation(
                        Handle,
                        handle =>
                        {
                            Span<INPUT_RECORD> records = stackalloc INPUT_RECORD[1];

                            // Ensure that we actually have a useful key event so that ReadConsole will not block.
                            //
                            // TODO: Discarding non-key events is gross. We should find a better way.
                            while (PeekConsoleInputW(handle, records, out var recordsRead) && recordsRead == 1)
                            {
                                var rec = MemoryMarshal.GetReference(records);
                                var evt = rec.Event.KeyEvent;

                                if (rec.EventType == KEY_EVENT && evt.bKeyDown &&
                                    (VIRTUAL_KEY)evt.wVirtualKeyCode is not
                                    VIRTUAL_KEY.VK_SHIFT or VIRTUAL_KEY.VK_CONTROL or VIRTUAL_KEY.VK_MENU or
                                    VIRTUAL_KEY.VK_CAPITAL or VIRTUAL_KEY.VK_NUMLOCK or VIRTUAL_KEY.VK_SCROLL)
                                    return true;

                                _ = ReadConsoleInputW(handle, records, out _);
                            }

                            return false;
                        },
                        cancellationToken);

                    Span<char> units = stackalloc char[2];
                    var chars = 0;

                    fixed (char* p = &MemoryMarshal.GetReference(units))
                    {
                        bool ret;
                        var read = 0u;

                        while ((ret = ReadConsoleW(Handle, p, 1, out read, null)) &&
                            Marshal.GetLastPInvokeError() == (int)WIN32_ERROR.ERROR_OPERATION_ABORTED &&
                            read == 0)
                        {
                            // Retry in case we get interrupted by a signal.
                        }

                        if (!ret)
                            WindowsTerminalUtility.ThrowIfUnexpected($"Could not read from {Name}");

                        if (read == 0)
                            return 0;

                        // There is a bug where ReadConsoleW will not process Ctrl-Z properly even though ReadFile will.
                        // The good news is that we can fairly easily emulate what the console host should be doing by
                        // just pretending that there is no more data to be read.
                        if (!Driver.IsRawMode && *p == '\x1a')
                            return 0;

                        chars++;

                        // If we got a high surrogate, we expect to instantly see a low surrogate following it. In
                        // really bizarre situations (e.g. broken WriteConsoleInput calls), this might not be the case
                        // though; in such a case, we will just let UTF8Encoding encode the lone high surrogate into a
                        // replacement character (U+FFFD).
                        //
                        // It is not really clear whether this is the right thing to do. A case could easily be made for
                        // passing the lone surrogate through unmodified or simply discarding it...
                        if (char.IsHighSurrogate(*p))
                        {
                            while ((ret = ReadConsoleW(Handle, p + 1, 1, out read, null)) &&
                                Marshal.GetLastPInvokeError() == (int)WIN32_ERROR.ERROR_OPERATION_ABORTED &&
                                read == 0)
                            {
                                // Retry in case we get interrupted by a signal.
                            }

                            // See comments in UnixTerminalWriter for why we are only throwing on a failed read that
                            // read nothing.
                            if (read != 0)
                                chars++;
                            else if (!ret)
                                WindowsTerminalUtility.ThrowIfUnexpected($"Could not read from {Name}");
                        }

                        // Encode the UTF-16 code unit(s) into UTF-8 and grab a slice of the buffer corresponding to
                        // just the portion used.
                        _buffered = _buffer.AsMemory(0, Terminal.Encoding.GetBytes(units[..chars], _buffer));
                    }
                }

                // Now that we have some UTF-8 text buffered up, we can copy it over to the buffer provided by the
                // caller and adjust our UTF-8 buffer accordingly. Be careful not to overrun either buffer.
                var copied = Math.Min(_buffered.Length, buffer.Length);

                _buffered.Span[..copied].CopyTo(buffer[..copied]);
                _buffered = _buffered[copied..];

                return copied;
            }
        }
        else
        {
            bool result;
            uint read;

            // Note that Windows does not support WaitForMultipleObjects on files, so there is no point in trying to
            // use WindowsCancellationEvent in this case.
            lock (_lock)
                fixed (byte* p = &MemoryMarshal.GetReference(buffer))
                    result = ReadFile(Handle, p, (uint)buffer.Length, &read, null);

            // See comments in UnixTerminalWriter for why we are only throwing on a failed read that read nothing.
            if (!result && read == 0)
                WindowsTerminalUtility.ThrowIfUnexpected($"Could not read from {Name}");

            return (int)read;
        }
    }
}
