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

    protected override unsafe int WriteBufferCore(ReadOnlySpan<byte> buffer, CancellationToken cancellationToken)
    {
        // If the handle is invalid, just present the illusion to the user that it has been redirected to /dev/null or
        // something along those lines, i.e. pretend we wrote everything.
        if (buffer.IsEmpty || !IsValid)
            return buffer.Length;

        cancellationToken.ThrowIfCancellationRequested();

        bool result;
        uint written;

        // Unlike Unix's write system call, if WriteFile returns a successful status, it means everything was written.
        // So, we do not need to do this in a loop.
        lock (_lock)
            fixed (byte* p = &MemoryMarshal.GetReference(buffer))
                result = WriteFile(Handle, p, (uint)buffer.Length, &written, null);

        // See comments in UnixTerminalWriter for why we are only throwing on a failed write that wrote nothing.
        if (!result && written == 0)
            WindowsTerminalUtility.ThrowIfUnexpected($"Could not write to {Name}");

        return (int)written;
    }
}
