using System.IO.Pipelines;

namespace System.IO;

public class TestTerminalWriter : TerminalWriter
{
    // TODO: Add synchronization.

    public override Stream Stream { get; }

    public override bool IsValid => _isValid;

    public override bool IsInteractive => _isInteractive;

    readonly Pipe _pipe = new(new(pauseWriterThreshold: 0, useSynchronizationContext: false));

    bool _isValid = true;

    bool _isInteractive = true;

    public TestTerminalWriter()
    {
        Stream = new TerminalOutputStream(this);
    }

    protected override sealed int WritePartialCore(ReadOnlySpan<byte> buffer, CancellationToken cancellationToken)
    {
        var len = buffer.Length;
        var array = ArrayPool<byte>.Shared.Rent(len);

        try
        {
            return WritePartialCoreAsync(array.AsMemory(..len), cancellationToken)
                .AsTask()
                .GetAwaiter()
                .GetResult();
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(array);
        }
    }

    protected override sealed async ValueTask<int> WritePartialCoreAsync(
        ReadOnlyMemory<byte> buffer,
        CancellationToken cancellationToken)
    {
        if (buffer.IsEmpty || !_isValid)
            return 0;

        _ = await _pipe.Writer.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);

        return buffer.Length;
    }

    public async ValueTask<int> ReadOutputAsync(Memory<byte> buffer)
    {
        if (buffer.IsEmpty)
            return 0;

        var result = await _pipe.Reader.ReadAtLeastAsync(1).ConfigureAwait(false);
        var seq = result.Buffer;
        var len = Math.Min(seq.Length, buffer.Length);

        seq.Slice(0, len).CopyTo(buffer.Span);

        _pipe.Reader.AdvanceTo(seq.GetPosition(len));

        return (int)len;
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
