namespace Vezel.Cathode.Terminals;

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

    protected override NativeTerminalReader CreateReader(nuint handle, SemaphoreSlim semaphore)
    {
        return new(this, handle, semaphore, cancellationHook: null);
    }

    protected override NativeTerminalWriter CreateWriter(nuint handle, SemaphoreSlim semaphore)
    {
        return new(this, handle, semaphore, cancellationHook: null);
    }
}
