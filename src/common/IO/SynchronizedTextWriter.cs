using Vezel.Cathode.Threading;

namespace Vezel.Cathode.IO;

[SuppressMessage("", "CA2213")]
internal sealed class SynchronizedTextWriter : TextWriter
{
    // The writer returned by TextWriter.Synchronized has no async support and also lacks forwarding for newer method
    // overloads. This class addresses those issues.
    //
    // Note that Close, Dispose, and DisposeAsync are intentionally not forwarded as this class is meant to wrap writers
    // that are disposed by other means or are intended to live for the duration of the program.

    public override Encoding Encoding => _writer.Encoding;

    public override IFormatProvider FormatProvider => _writer.FormatProvider;

    [AllowNull]
    public override string NewLine
    {
        get
        {
            using (_lock.Enter())
                return _writer.NewLine;
        }

        set
        {
            using (_lock.Enter())
                _writer.NewLine = value;
        }
    }

    private readonly SemaphoreSlim _lock = new(1, 1);

    private readonly TextWriter _writer;

    public SynchronizedTextWriter(TextWriter writer)
    {
        _writer = writer;
    }

    public override void Flush()
    {
        using (_lock.Enter())
            _writer.Flush();
    }

    public override async Task FlushAsync()
    {
        using (await _lock.EnterAsync().ConfigureAwait(false))
            await _writer.FlushAsync().ConfigureAwait(false);
    }

    public override async Task FlushAsync(CancellationToken cancellationToken)
    {
        using (await _lock.EnterAsync(cancellationToken).ConfigureAwait(false))
            await _writer.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    public override void Write(char value)
    {
        using (_lock.Enter())
            _writer.Write(value);
    }

    public override void Write(char[]? buffer)
    {
        using (_lock.Enter())
            _writer.Write(buffer);
    }

    public override void Write(char[] buffer, int index, int count)
    {
        using (_lock.Enter())
            _writer.Write(buffer, index, count);
    }

    public override void Write(ReadOnlySpan<char> buffer)
    {
        using (_lock.Enter())
            _writer.Write(buffer);
    }

    public override void Write(bool value)
    {
        using (_lock.Enter())
            _writer.Write(value);
    }

    public override void Write(int value)
    {
        using (_lock.Enter())
            _writer.Write(value);
    }

    public override void Write(uint value)
    {
        using (_lock.Enter())
            _writer.Write(value);
    }

    public override void Write(long value)
    {
        using (_lock.Enter())
            _writer.Write(value);
    }

    public override void Write(ulong value)
    {
        using (_lock.Enter())
            _writer.Write(value);
    }

    public override void Write(float value)
    {
        using (_lock.Enter())
            _writer.Write(value);
    }

    public override void Write(double value)
    {
        using (_lock.Enter())
            _writer.Write(value);
    }

    public override void Write(decimal value)
    {
        using (_lock.Enter())
            _writer.Write(value);
    }

    public override void Write(string? value)
    {
        using (_lock.Enter())
            _writer.Write(value);
    }

    public override void Write(object? value)
    {
        using (_lock.Enter())
            _writer.Write(value);
    }

    public override void Write(StringBuilder? value)
    {
        using (_lock.Enter())
            _writer.Write(value);
    }

    public override void Write(string format, object? arg0)
    {
        using (_lock.Enter())
            _writer.Write(format, arg0);
    }

    public override void Write(string format, object? arg0, object? arg1)
    {
        using (_lock.Enter())
            _writer.Write(format, arg0, arg1);
    }

    public override void Write(string format, object? arg0, object? arg1, object? arg2)
    {
        using (_lock.Enter())
            _writer.Write(format, arg0, arg1, arg2);
    }

    public override void Write(string format, params object?[] arg)
    {
        using (_lock.Enter())
            _writer.Write(format, arg);
    }

    public override async Task WriteAsync(char value)
    {
        using (await _lock.EnterAsync().ConfigureAwait(false))
            await _writer.WriteAsync(value).ConfigureAwait(false);
    }

    public override async Task WriteAsync(char[] buffer, int index, int count)
    {
        using (await _lock.EnterAsync().ConfigureAwait(false))
            await _writer.WriteAsync(buffer, index, count).ConfigureAwait(false);
    }

    public override async Task WriteAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
    {
        using (await _lock.EnterAsync(cancellationToken).ConfigureAwait(false))
            await _writer.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
    }

    public override async Task WriteAsync(string? value)
    {
        using (await _lock.EnterAsync().ConfigureAwait(false))
            await _writer.WriteAsync(value).ConfigureAwait(false);
    }

    public override async Task WriteAsync(StringBuilder? value, CancellationToken cancellationToken = default)
    {
        using (await _lock.EnterAsync(cancellationToken).ConfigureAwait(false))
            await _writer.WriteAsync(value, cancellationToken).ConfigureAwait(false);
    }

    public override void WriteLine()
    {
        using (_lock.Enter())
            _writer.WriteLine();
    }

    public override void WriteLine(char value)
    {
        using (_lock.Enter())
            _writer.WriteLine(value);
    }

    public override void WriteLine(char[]? buffer)
    {
        using (_lock.Enter())
            _writer.WriteLine(buffer);
    }

    public override void WriteLine(char[] buffer, int index, int count)
    {
        using (_lock.Enter())
            _writer.WriteLine(buffer, index, count);
    }

    public override void WriteLine(ReadOnlySpan<char> buffer)
    {
        using (_lock.Enter())
            _writer.WriteLine(buffer);
    }

    public override void WriteLine(bool value)
    {
        using (_lock.Enter())
            _writer.WriteLine(value);
    }

    public override void WriteLine(int value)
    {
        using (_lock.Enter())
            _writer.WriteLine(value);
    }

    public override void WriteLine(uint value)
    {
        using (_lock.Enter())
            _writer.WriteLine(value);
    }

    public override void WriteLine(long value)
    {
        using (_lock.Enter())
            _writer.WriteLine(value);
    }

    public override void WriteLine(ulong value)
    {
        using (_lock.Enter())
            _writer.WriteLine(value);
    }

    public override void WriteLine(float value)
    {
        using (_lock.Enter())
            _writer.WriteLine(value);
    }

    public override void WriteLine(double value)
    {
        using (_lock.Enter())
            _writer.WriteLine(value);
    }

    public override void WriteLine(decimal value)
    {
        using (_lock.Enter())
            _writer.WriteLine(value);
    }

    public override void WriteLine(string? value)
    {
        using (_lock.Enter())
            _writer.WriteLine(value);
    }

    public override void WriteLine(StringBuilder? value)
    {
        using (_lock.Enter())
            _writer.WriteLine(value);
    }

    public override void WriteLine(object? value)
    {
        using (_lock.Enter())
            _writer.WriteLine(value);
    }

    public override void WriteLine(string format, object? arg0)
    {
        using (_lock.Enter())
            _writer.WriteLine(format, arg0);
    }

    public override void WriteLine(string format, object? arg0, object? arg1)
    {
        using (_lock.Enter())
            _writer.WriteLine(format, arg0, arg1);
    }

    public override void WriteLine(string format, object? arg0, object? arg1, object? arg2)
    {
        using (_lock.Enter())
            _writer.WriteLine(format, arg0, arg1, arg2);
    }

    public override void WriteLine(string format, params object?[] arg)
    {
        using (_lock.Enter())
            _writer.WriteLine(format, arg);
    }

    public override async Task WriteLineAsync()
    {
        using (await _lock.EnterAsync().ConfigureAwait(false))
            await _writer.WriteLineAsync().ConfigureAwait(false);
    }

    public override async Task WriteLineAsync(char value)
    {
        using (await _lock.EnterAsync().ConfigureAwait(false))
            await _writer.WriteLineAsync(value).ConfigureAwait(false);
    }

    public override async Task WriteLineAsync(char[] buffer, int index, int count)
    {
        using (await _lock.EnterAsync().ConfigureAwait(false))
            await _writer.WriteLineAsync(buffer, index, count).ConfigureAwait(false);
    }

    public override async Task WriteLineAsync(
        ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
    {
        using (await _lock.EnterAsync(cancellationToken).ConfigureAwait(false))
            await _writer.WriteLineAsync(buffer, cancellationToken).ConfigureAwait(false);
    }

    public override async Task WriteLineAsync(string? value)
    {
        using (await _lock.EnterAsync().ConfigureAwait(false))
            await _writer.WriteLineAsync(value).ConfigureAwait(false);
    }

    public override async Task WriteLineAsync(StringBuilder? value, CancellationToken cancellationToken = default)
    {
        using (await _lock.EnterAsync(cancellationToken).ConfigureAwait(false))
            await _writer.WriteLineAsync(value, cancellationToken).ConfigureAwait(false);
    }
}
