namespace System.IO;

abstract class DefaultTerminalReader : TerminalReader
{
    public string Name { get; }

    public override sealed TerminalInputStream Stream { get; }

    protected DefaultTerminalReader(string name)
    {
        Name = name;
        Stream = new(this);
    }
}
