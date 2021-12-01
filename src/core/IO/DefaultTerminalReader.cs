namespace System.IO;

abstract class DefaultTerminalReader : TerminalReader
{
    public override sealed TerminalInputStream Stream { get; }

    protected DefaultTerminalReader()
    {
        Stream = new(this);
    }
}
