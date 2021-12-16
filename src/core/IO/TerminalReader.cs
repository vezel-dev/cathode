namespace System.IO;

public abstract class TerminalReader : TerminalHandle
{
    const int ReadBufferSize = 4096;

    public event SpanAction<byte, TerminalReader>? InputRead;

    public TextReader TextReader => _reader.Value;

    readonly Lazy<TextReader> _reader;

    [SuppressMessage("Reliability", "CA2000")]
    protected TerminalReader()
    {
        // Note that the buffer size used affects how many characters the Windows console host will allow the user to
        // type in a single line (in cooked mode).
        _reader = new(
            () => TextReader.Synchronized(new StreamReader(Stream, Terminal.Encoding, false, ReadBufferSize, true)));
    }

    protected abstract void ReadCore(Span<byte> data, out int count, CancellationToken cancellationToken);

    public void Read(Span<byte> data, out int count, CancellationToken cancellationToken = default)
    {
        count = 0;

        // ReadCore is required to assign count appropriately for partial reads that fail.
        try
        {
            ReadCore(data, out count, cancellationToken);
        }
        finally
        {
            InputRead?.Invoke(data[..count], this);
        }
    }

    public byte? ReadRaw(CancellationToken cancellationToken = default)
    {
        Span<byte> span = stackalloc byte[1];

        Read(span, out var count, cancellationToken);

        return count == span.Length ? span[0] : null;
    }

    public string? ReadLine()
    {
        return TextReader.ReadLine();
    }
}
