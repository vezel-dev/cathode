namespace System.IO;

public abstract class TerminalReader : TerminalHandle
{
    public event SpanAction<byte, TerminalReader>? InputRead;

    readonly Lazy<StreamReader> _reader;

    protected TerminalReader()
    {
        _reader = new(() => new(Stream, Terminal.Encoding, false, Environment.SystemPageSize, true));
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
        return _reader.Value.ReadLine();
    }
}
