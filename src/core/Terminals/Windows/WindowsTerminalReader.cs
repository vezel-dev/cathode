using static Windows.Win32.WindowsPInvoke;

namespace Vezel.Cathode.Terminals.Windows;

internal sealed class WindowsTerminalReader : NativeTerminalReader<WindowsVirtualTerminal, SafeHandle>
{
    private readonly SemaphoreSlim _semaphore;

    public WindowsTerminalReader(
        WindowsVirtualTerminal terminal, string name, SafeHandle handle, SemaphoreSlim semaphore)
        : base(terminal, name, handle)
    {
        _semaphore = semaphore;
    }

    protected override unsafe int ReadPartialNative(scoped Span<byte> buffer, CancellationToken cancellationToken)
    {
        using var guard = Terminal.Control.Guard();

        // If the handle is invalid, just present the illusion to the user that it has been redirected to /dev/null or
        // something along those lines, i.e. return EOF.
        if (buffer.IsEmpty || !IsValid)
            return 0;

        bool result;
        uint read;

        using (_semaphore.Enter(cancellationToken))
            result = ReadFile(Handle, buffer, &read, null);

        if (!result && read == 0)
            WindowsTerminalUtility.ThrowIfUnexpected($"Could not read from {Name}");

        return (int)read;
    }
}
