namespace System.IO;

abstract class DefaultTerminalWriter : TerminalWriter
{
    public override TerminalOutputStream Stream { get; }

    protected DefaultTerminalWriter()
    {
        Stream = new(this);
    }
}
