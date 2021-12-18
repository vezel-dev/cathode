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

    protected abstract int ReadBufferCore(Span<byte> buffer, CancellationToken cancellationToken);

    public int ReadBuffer(Span<byte> buffer, CancellationToken cancellationToken = default)
    {
        var count = ReadBufferCore(buffer, cancellationToken);

        InputRead?.Invoke(buffer[..count], this);

        return count;
    }

    public int Read(Span<byte> value, CancellationToken cancellationToken = default)
    {
        var count = 0;

        while (count < value.Length)
        {
            var ret = ReadBuffer(value[count..], cancellationToken);

            // EOF?
            if (ret == 0)
                break;

            count += ret;
        }

        return count;
    }

    public unsafe byte? ReadRaw(CancellationToken cancellationToken = default)
    {
        byte value;

        return Read(new Span<byte>(&value, 1), cancellationToken) == 1 ? value : null;
    }

    public string? ReadLine()
    {
        return TextReader.ReadLine();
    }
}
