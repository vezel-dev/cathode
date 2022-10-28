namespace Vezel.Cathode;

public sealed class TerminalSignalContext
{
    public TerminalSignal Signal { get; }

    public bool Cancel { get; set; }

    public TerminalSignalContext(TerminalSignal signal)
    {
        Check.Enum(signal);

        Signal = signal;
    }
}
