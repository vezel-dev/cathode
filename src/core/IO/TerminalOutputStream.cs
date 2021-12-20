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
        // Stream documentation requires us to write the entire buffer until complete or an exception occurs.
        Writer.Write(buffer);
    }

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        return Writer.WriteAsync(buffer, cancellationToken);
    }
}
