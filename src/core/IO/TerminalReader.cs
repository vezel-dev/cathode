namespace System.IO;

public abstract class TerminalReader : TerminalHandle
{
    const int ReadBufferSize = 4096;

    public event SpanAction<byte, TerminalReader>? InputRead;

    public TextReader Reader => _reader.Value;

    readonly Lazy<TextReader> _reader;

    [SuppressMessage("Reliability", "CA2000")]
    protected TerminalReader()
    {
        // Note that the buffer size used affects how many characters the Windows console host will allow the user to
        // type in a single line (in cooked mode).
        _reader = new(
            () => TextReader.Synchronized(new StreamReader(Stream, Terminal.Encoding, false, ReadBufferSize, true)));
    }

    protected abstract void ReadCore(Span<byte> data, out int count);

    public void Read(Span<byte> data, out int count)
    {
        count = 0;

        // ReadCore is required to assign count appropriately for partial reads that fail.
        try
        {
            ReadCore(data, out count);
        }
        finally
        {
            InputRead?.Invoke(data[..count], this);
        }
    }

    public byte? ReadRaw()
    {
        Span<byte> span = stackalloc byte[1];

        Read(span, out var count);

        return count == span.Length ? span[0] : null;
    }

    public string? ReadLine()
    {
        return Reader.ReadLine();
    }
}
