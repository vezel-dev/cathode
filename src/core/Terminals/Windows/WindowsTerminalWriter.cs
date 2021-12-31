using static Windows.Win32.WindowsPInvoke;

namespace System.Terminals.Windows;

sealed class WindowsTerminalWriter : NativeTerminalWriter<WindowsVirtualTerminal, SafeHandle>
{
    readonly object _lock;

    public WindowsTerminalWriter(WindowsVirtualTerminal terminal, string name, SafeHandle handle, object @lock)
        : base(terminal, name, handle)
    {
        _lock = @lock;
    }

    protected override unsafe int WritePartialCore(ReadOnlySpan<byte> buffer, CancellationToken cancellationToken)
    {
        using var guard = Terminal.Control.Guard();

        // If the handle is invalid, just present the illusion to the user that it has been redirected to /dev/null or
        // something along those lines, i.e. pretend we wrote everything.
        if (buffer.IsEmpty || !IsValid)
            return buffer.Length;

        cancellationToken.ThrowIfCancellationRequested();

        bool result;
        uint written;

        lock (_lock)
            fixed (byte* p = &MemoryMarshal.GetReference(buffer))
                result = WriteFile(Handle, p, (uint)buffer.Length, &written, null);

        if (!result && written == 0)
            WindowsTerminalUtility.ThrowIfUnexpected($"Could not write to {Name}");

        return (int)written;
    }
}
