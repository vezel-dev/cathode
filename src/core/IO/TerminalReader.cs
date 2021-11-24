namespace System.IO;

public abstract class TerminalReader : TerminalHandle
{
    public event SpanAction<byte, TerminalReader>? InputRead;

    readonly Lazy<StreamReader> _reader;

    protected TerminalReader()
    {
        _reader = new(() => new(Stream, Terminal.Encoding, false, Environment.SystemPageSize, true));
    }

    protected abstract int ReadCore(Span<byte> data);

    public int Read(Span<byte> data)
    {
        var len = ReadCore(data);

        InputRead?.Invoke(data[..len], this);

        return len;
    }

    public byte? ReadRaw()
    {
        Span<byte> span = stackalloc byte[1];

        return Read(span) == span.Length ? span[0] : null;
    }

    public string? ReadLine()
    {
        return _reader.Value.ReadLine();
    }
}
