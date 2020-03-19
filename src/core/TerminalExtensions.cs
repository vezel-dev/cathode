using System.IO;

namespace System
{
    public static class TerminalExtensions
    {
        public static byte? ReadRaw(this TerminalReader reader)
        {
            _ = reader ?? throw new ArgumentNullException(nameof(reader));

            return reader.Driver.ReadRaw();
        }

        public static string? ReadLine(this TerminalReader reader)
        {
            _ = reader ?? throw new ArgumentNullException(nameof(reader));

            return reader.Driver.ReadLine();
        }

        public static void WriteBinary(this TerminalWriter writer, ReadOnlySpan<byte> value)
        {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));

            writer.Write(value);
        }

        public static void WriteText(this TerminalWriter writer, ReadOnlySpan<char> value)
        {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));

            TerminalUtility.EncodeAndExecute(value, writer.Encoding, span => writer.WriteBinary(span));
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
            writer.Write(string.Format(format, args));
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
            writer.WriteLine(string.Format(format, args));
        }
    }
}
