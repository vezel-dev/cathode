namespace System;

public sealed class TerminalSignalEventArgs : EventArgs
{
    public TerminalSignal Signal { get; }

    public bool Cancel { get; set; }

    internal TerminalSignalEventArgs(TerminalSignal signal)
    {
        Signal = signal;
    }
}
