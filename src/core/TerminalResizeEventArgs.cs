namespace System;

public sealed class TerminalResizeEventArgs : EventArgs
{
    public TerminalSize Size { get; }

    internal TerminalResizeEventArgs(TerminalSize size)
    {
        Size = size;
    }
}
