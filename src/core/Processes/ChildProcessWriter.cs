namespace System.Processes;

public sealed class ChildProcessWriter
{
    public Stream Stream { get; }

    public Encoding Encoding { get; }

    public TextWriter TextWriter { get; }

    internal ChildProcessWriter(StreamWriter writer)
    {
        Stream = writer.BaseStream;
        Encoding = writer.Encoding;
        TextWriter = writer;
    }
}
