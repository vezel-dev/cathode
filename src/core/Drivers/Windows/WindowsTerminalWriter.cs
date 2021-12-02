using Windows.Win32.System.Console;
using static Windows.Win32.WindowsPInvoke;

namespace System.Drivers.Windows;

sealed class WindowsTerminalWriter : DriverTerminalWriter<WindowsTerminalDriver, SafeHandle>
{
    readonly object _lock;

    public WindowsTerminalWriter(WindowsTerminalDriver driver, string name, SafeHandle handle, object @lock)
        : base(driver, name, handle)
    {
        _lock = @lock;
    }

    protected override unsafe void WriteCore(ReadOnlySpan<byte> data, out int count)
    {
        // If the handle is invalid, just present the illusion to the user that it has been redirected to /dev/null or
        // something along those lines, i.e. pretend we wrote everything.
        if (data.IsEmpty || !IsValid)
        {
            count = data.Length;

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
            WindowsTerminalUtility.ThrowIfUnexpected($"Could not write to {Name}");
    }
}
