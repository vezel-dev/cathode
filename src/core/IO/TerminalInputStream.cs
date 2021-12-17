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
        Reader.Read(buffer, out var count);

        return count;
    }

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        return cancellationToken.IsCancellationRequested ?
            ValueTask.FromCanceled<int>(cancellationToken) :
            new(Task.Run(
                () =>
                {
                    Reader.Read(buffer.Span, out var count, cancellationToken);

                    return count;
                },
                cancellationToken));
    }
}
