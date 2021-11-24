using Windows.Win32.System.Console;
using static Windows.Win32.WindowsPInvoke;

namespace System.Drivers.Windows;

sealed class WindowsTerminalWriter : DefaultTerminalWriter
{
    public SafeHandle Handle { get; }

    public bool IsValid { get; }

    public override bool IsRedirected { get; }

    readonly object _lock = new();

    readonly string _name;

    public WindowsTerminalWriter(SafeHandle handle, string name)
    {
        Handle = handle;
        IsValid = WindowsTerminalUtility.IsHandleValid(handle, true);
        IsRedirected = WindowsTerminalUtility.IsHandleRedirected(handle);
        _name = name;
    }

    protected override unsafe void WriteCore(ReadOnlySpan<byte> data, out int count)
    {
        if (data.IsEmpty || !IsValid)
        {
            count = 0;

            return;
        }

        bool result;
        uint written;

        // Unlike Unix's write system call, if WriteFile returns a successful status, it means everything was written.
        // So, we do not need to do this in a loop.
        lock (_lock)
            fixed (byte* p = data)
                result = WriteFile(Handle, p, (uint)data.Length, &written, null);

        count = (int)written;

        if (!result)
            WindowsTerminalUtility.ThrowIfUnexpected($"Could not write to standard {_name}");
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
