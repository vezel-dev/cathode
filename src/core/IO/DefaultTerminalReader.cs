namespace System.IO;

abstract class DefaultTerminalReader : TerminalReader
{
    public override TerminalInputStream Stream { get; }

    protected DefaultTerminalReader()
    {
        Stream = new(this);
    }
}
