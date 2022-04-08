using Vezel.Cathode.Threading;
using static Windows.Win32.WindowsPInvoke;

namespace Vezel.Cathode.Terminals.Windows;

sealed class WindowsTerminalWriter : NativeTerminalWriter<WindowsVirtualTerminal, SafeHandle>
{
    readonly SemaphoreSlim _semaphore;

    public WindowsTerminalWriter(
        WindowsVirtualTerminal terminal,
        string name,
        SafeHandle handle,
        SemaphoreSlim semaphore)
        : base(terminal, name, handle)
    {
        _semaphore = semaphore;
    }

    protected override unsafe int WritePartialCore(ReadOnlySpan<byte> buffer, CancellationToken cancellationToken)
    {
        using var guard = Terminal.Control.Guard();

        // If the handle is invalid, just present the illusion to the user that it has been redirected to /dev/null or
        // something along those lines, i.e. pretend we wrote everything.
        if (buffer.IsEmpty || !IsValid)
            return buffer.Length;

        bool result;
        uint written;

        using (_semaphore.Enter(cancellationToken))
            fixed (byte* p = &MemoryMarshal.GetReference(buffer))
                result = WriteFile(Handle, p, (uint)buffer.Length, &written, null);

        if (!result && written == 0)
            WindowsTerminalUtility.ThrowIfUnexpected($"Could not write to {Name}");

        return (int)written;
    }
}
