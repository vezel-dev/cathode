namespace System.IO;

public class TestTerminalReader : TerminalReader
{
    // TODO: Add synchronization.

    public override sealed Stream Stream { get; }

    public override sealed TextReader TextReader { get; }

    public override sealed bool IsValid => _isValid;

    public override sealed bool IsInteractive => _isInteractive;

    readonly Pipe _pipe = new(new(pauseWriterThreshold: 0, useSynchronizationContext: false));

    bool _isValid = true;

    bool _isInteractive = true;

    public TestTerminalReader()
    {
        Stream = new TerminalInputStream(this);
        TextReader = new StreamReader(Stream, Terminal.Encoding, false);
    }

    [SuppressMessage("Usage", "VSTHRD002")]
    protected override sealed int ReadPartialCore(Span<byte> buffer, CancellationToken cancellationToken)
    {
        var len = buffer.Length;
        var array = ArrayPool<byte>.Shared.Rent(len);

        try
        {
            var read = ReadPartialCoreAsync(array.AsMemory(..len), cancellationToken)
                .AsTask()
                .GetAwaiter()
                .GetResult();

            array.AsSpan(..read).CopyTo(buffer);

            return read;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(array);
        }
    }

    protected override sealed async ValueTask<int> ReadPartialCoreAsync(
        Memory<byte> buffer,
        CancellationToken cancellationToken)
    {
        if (buffer.IsEmpty || !_isValid)
            return 0;

        var result = await _pipe.Reader.ReadAtLeastAsync(1, cancellationToken).ConfigureAwait(false);
        var seq = result.Buffer;
        var len = Math.Min(seq.Length, buffer.Length);

        seq.Slice(0, len).CopyTo(buffer.Span);

        _pipe.Reader.AdvanceTo(seq.GetPosition(len));

        return (int)len;
    }

    public async ValueTask WriteInputAsync(ReadOnlyMemory<byte> buffer)
    {
        _ = await _pipe.Writer.WriteAsync(buffer).ConfigureAwait(false);
    }

    public void SetValid(bool value)
    {
        _isValid = value;
    }

    public void SetInteractive(bool value)
    {
        _isInteractive = value;
    }
}
