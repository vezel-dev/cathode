namespace System;

public sealed class TerminalSignalContext
{
    public TerminalSignal Signal { get; }

    public bool Cancel { get; set; }

    public TerminalSignalContext(TerminalSignal signal)
    {
        _ = Enum.IsDefined(signal) ? true : throw new ArgumentOutOfRangeException(nameof(signal));

        Signal = signal;
    }
}
