namespace System.IO;

abstract class DefaultTerminalWriter : TerminalWriter
{
    public override sealed TerminalOutputStream Stream { get; }

    protected DefaultTerminalWriter()
    {
        Stream = new(this);
    }
}
