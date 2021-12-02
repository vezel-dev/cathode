namespace System.IO;

abstract class DefaultTerminalWriter : TerminalWriter
{
    public string Name { get; }

    public override sealed TerminalOutputStream Stream { get; }

    protected DefaultTerminalWriter(string name)
    {
        Name = name;
        Stream = new(this);
    }
}
