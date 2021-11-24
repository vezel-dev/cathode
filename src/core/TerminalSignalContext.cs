namespace System;

public sealed class TerminalSignalContext
{
    public TerminalSignal Signal { get; }

    public bool Cancel { get; set; }

    internal TerminalSignalContext(TerminalSignal signal)
    {
        Signal = signal;
    }
}
