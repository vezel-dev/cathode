namespace Vezel.Cathode.Terminals.Windows;

internal sealed class WindowsVirtualTerminal : NativeVirtualTerminal
{
    public override event Action? Resumed
    {
        add
        {
            // Windows does not have a SIGSTOP/SIGCONT concept.
        }

        remove
        {
        }
    }

    public static WindowsVirtualTerminal Instance { get; } = new();

    private WindowsVirtualTerminal()
    {
    }

    protected override WindowsTerminalReader CreateReader(nuint handle, SemaphoreSlim semaphore)
    {
        return new(this, handle, semaphore);
    }

    protected override WindowsTerminalWriter CreateWriter(nuint handle, SemaphoreSlim semaphore)
    {
        return new(this, handle, semaphore);
    }
}
