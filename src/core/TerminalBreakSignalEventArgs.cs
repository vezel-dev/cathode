namespace System;

public sealed class TerminalBreakSignalEventArgs : EventArgs
{
    public TerminalBreakSignal Signal { get; }

    public bool Cancel { get; set; }

    internal TerminalBreakSignalEventArgs(TerminalBreakSignal signal)
    {
        Signal = signal;
    }
}
