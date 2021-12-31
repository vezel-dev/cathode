namespace System.IO;

public sealed class TerminalInputStream : TerminalStream
{
    public TerminalReader Reader { get; }

    public override bool CanRead => true;

    public override bool CanWrite => false;

    public TerminalInputStream(TerminalReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        Reader = reader;
    }

    public override int Read(Span<byte> buffer)
    {
        // Unlike for writing, Stream documentation allows us to only do the bare minimum when reading.
        return Reader.ReadPartial(buffer);
    }

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        return Reader.ReadPartialAsync(buffer, cancellationToken);
    }
}
