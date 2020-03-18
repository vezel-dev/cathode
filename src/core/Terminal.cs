using System.Drivers;
using System.IO;
using System.Runtime.InteropServices;

namespace System
{
    public static class Terminal
    {
        public static event EventHandler<TerminalBreakEventArgs>? Break
        {
            add => _driver.Break += value;
            remove => _driver.Break -= value;
        }

        public static TerminalReader StdIn => _driver.StdIn;

        public static TerminalWriter StdOut => _driver.StdOut;

        public static TerminalWriter StdError => _driver.StdError;

        public static bool IsRawMode => _driver.IsRawMode;

        public static (int Width, int Height) Size => _driver.Size;

        public static string Title
        {
            get => _driver.Title;
            set => _driver.Title = value;
        }

        static readonly TerminalDriver _driver = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
            (TerminalDriver)WindowsTerminalDriver.Instance : UnixTerminalDriver.Instance;

        public static void SetRawMode(bool raw, bool discard)
        {
            _driver.SetRawMode(raw, discard);
        }

        public static void Clear(bool cursor = true)
        {
            _driver.Clear(cursor);
        }

        public static void Beep()
        {
            _driver.Beep();
        }

        public static byte? ReadRaw()
        {
            return StdIn.ReadRaw();
        }

        public static string? ReadLine()
        {
            return StdIn.ReadLine();
        }

        public static void OutBinary(ReadOnlySpan<byte> value)
        {
            StdOut.WriteBinary(value);
        }

        public static void OutText(ReadOnlySpan<char> value)
        {
            StdOut.WriteText(value);
        }

        public static void Out(string? value)
        {
            StdOut.Write(value);
        }

        public static void Out<T>(T value)
        {
            StdOut.Write(value);
        }

        public static void Out(string format, params object?[] args)
        {
            StdOut.Write(format, args);
        }

        public static void OutLine()
        {
            StdOut.WriteLine();
        }

        public static void OutLine(string? value)
        {
            StdOut.WriteLine(value);
        }

        public static void OutLine<T>(T value)
        {
            StdOut.WriteLine(value);
        }

        public static void OutLine(string format, params object?[] args)
        {
            StdOut.WriteLine(format, args);
        }

        public static void OutForegroundColor(byte r, byte g, byte b)
        {
            StdOut.ForegroundColor(r, g, b);
        }

        public static void OutBackgroundColor(byte r, byte g, byte b)
        {
            StdOut.BackgroundColor(r, g, b);
        }

        public static void OutResetAttributes()
        {
            StdOut.ResetAttributes();
        }

        public static void ErrorBinary(ReadOnlySpan<byte> value)
        {
            StdError.WriteBinary(value);
        }

        public static void ErrorText(ReadOnlySpan<char> value)
        {
            StdError.WriteText(value);
        }

        public static void Error(string? value)
        {
            StdError.Write(value);
        }

        public static void Error<T>(T value)
        {
            StdError.Write(value);
        }

        public static void Error(string format, params object?[] args)
        {
            StdError.Write(format, args);
        }

        public static void ErrorLine()
        {
            StdError.WriteLine();
        }

        public static void ErrorLine(string? value)
        {
            StdError.WriteLine(value);
        }

        public static void ErrorLine<T>(T value)
        {
            StdError.WriteLine(value);
        }

        public static void ErrorLine(string format, params object?[] args)
        {
            StdError.WriteLine(format, args);
        }

        public static void ErrorForegroundColor(byte r, byte g, byte b)
        {
            StdError.ForegroundColor(r, g, b);
        }

        public static void ErrorBackgroundColor(byte r, byte g, byte b)
        {
            StdError.BackgroundColor(r, g, b);
        }

        public static void ErrorResetAttributes()
        {
            StdError.ResetAttributes();
        }
    }
}
