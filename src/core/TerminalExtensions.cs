namespace System;

public static class TerminalExtensions
{
    public static byte? ReadRaw(this TerminalReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        return reader.Driver.ReadRaw();
    }

    public static string? ReadLine(this TerminalReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        return reader.Driver.ReadLine();
    }

    public static void WriteBinary(this TerminalWriter writer, ReadOnlySpan<byte> value)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.Write(value);
    }

    public static void WriteText(this TerminalWriter writer, ReadOnlySpan<char> value)
    {
        ArgumentNullException.ThrowIfNull(writer);

        var len = writer.Encoding.GetByteCount(value);
        var array = ArrayPool<byte>.Shared.Rent(len);

        try
        {
            var span = array.AsSpan(0, len);

            _ = writer.Encoding.GetBytes(value, span);

            writer.WriteBinary(span);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(array);
        }
    }

    public static void Write(this TerminalWriter writer, string? value)
    {
        writer.WriteText(value.AsSpan());
    }

    public static void Write<T>(this TerminalWriter writer, T value)
    {
        writer.Write(value?.ToString());
    }

    public static void Write(this TerminalWriter writer, string format, params object?[] args)
    {
        writer.Write(string.Format(CultureInfo.CurrentCulture, format, args));
    }

    public static void WriteLine(this TerminalWriter writer)
    {
        writer.Write(Environment.NewLine);
    }

    public static void WriteLine(this TerminalWriter writer, string? value)
    {
        writer.Write(value + Environment.NewLine);
    }

    public static void WriteLine<T>(this TerminalWriter writer, T value)
    {
        writer.WriteLine(value?.ToString());
    }

    public static void WriteLine(this TerminalWriter writer, string format, params object?[] args)
    {
        writer.WriteLine(string.Format(CultureInfo.CurrentCulture, format, args));
    }
}
