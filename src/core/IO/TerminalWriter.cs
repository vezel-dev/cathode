namespace System.IO;

public abstract class TerminalWriter : TerminalHandle
{
    public event ReadOnlySpanAction<byte, TerminalWriter>? OutputWritten;

    protected abstract void WriteCore(ReadOnlySpan<byte> data);

    public void Write(ReadOnlySpan<byte> data)
    {
        WriteCore(data);

        OutputWritten?.Invoke(data, this);
    }

    public void Write(ReadOnlySpan<char> value)
    {
        var len = Terminal.Encoding.GetByteCount(value);
        var array = ArrayPool<byte>.Shared.Rent(len);

        try
        {
            var span = array.AsSpan(0, len);

            _ = Terminal.Encoding.GetBytes(value, span);

            Write(span);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(array);
        }
    }

    public void Write(string? value)
    {
        Write(value.AsSpan());
    }

    public void Write<T>(T value)
    {
        Write(value?.ToString());
    }

    public void Write(string format, params object?[] args)
    {
        Write(string.Format(CultureInfo.CurrentCulture, format, args));
    }

    public void WriteLine()
    {
        WriteLine(null);
    }

    public void WriteLine(string? value)
    {
        Write(value + Environment.NewLine);
    }

    public void WriteLine<T>(T value)
    {
        WriteLine(value?.ToString());
    }

    public void WriteLine(string format, params object?[] args)
    {
        WriteLine(string.Format(CultureInfo.CurrentCulture, format, args));
    }
}
