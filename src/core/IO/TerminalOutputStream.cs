namespace System.IO;

public sealed class TerminalOutputStream : TerminalStream
{
    public TerminalWriter Writer { get; }

    public override bool CanRead => false;

    public override bool CanWrite => true;

    public TerminalOutputStream(TerminalWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        Writer = writer;
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        Writer.Write(buffer);
    }
}
