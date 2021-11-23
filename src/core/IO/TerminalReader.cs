namespace System.IO;

public abstract class TerminalReader : TerminalHandle
{
    readonly Lazy<StreamReader> _reader;

    protected TerminalReader()
    {
        _reader = new(() => new(Stream, Terminal.Encoding, false, Environment.SystemPageSize, true));
    }

    public abstract int Read(Span<byte> data);

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
