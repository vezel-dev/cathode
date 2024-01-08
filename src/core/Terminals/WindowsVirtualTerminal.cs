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

    protected override unsafe Action<nuint, CancellationToken>? CreateCancellationHook(bool write)
    {
        return null;
    }
}
