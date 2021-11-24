using System.Drivers;
using System.Drivers.Unix;
using System.Drivers.Windows;

namespace System;

public static class Terminal
{
    public static event EventHandler<TerminalResizeEventArgs>? Resize
    {
        add => _driver.Resize += value;
        remove => _driver.Resize -= value;
    }

    public static event EventHandler<TerminalSignalEventArgs>? Signal
    {
        add => _driver.Signal += value;
        remove => _driver.Signal -= value;
    }

    public static Encoding Encoding { get; } = new UTF8Encoding(false);

    public static TerminalReader StdIn => _driver.StdIn;

    public static TerminalWriter StdOut => _driver.StdOut;

    public static TerminalWriter StdError => _driver.StdError;

    public static bool IsRawMode => _driver.IsRawMode;

    public static TerminalMouseEvents MouseEvents => _driver.MouseEvents;

    public static string Title
    {
        get => _driver.Title;
        set => _driver.Title = value;
    }

    public static TerminalSize Size => _driver.Size;

    public static TerminalKeyMode CursorKeyMode
    {
        get => _driver.CursorKeyMode;
        set => _driver.CursorKeyMode = value;
    }

    public static TerminalKeyMode NumericKeyMode
    {
        get => _driver.NumericKeyMode;
        set => _driver.NumericKeyMode = value;
    }

    public static bool IsCursorVisible
    {
        get => Screen.IsCursorVisible;
        set => Screen.IsCursorVisible = value;
    }

    public static TerminalCursorStyle CursorStyle
    {
        get => Screen.CursorStyle;
        set => Screen.CursorStyle = value;
    }

    public static TerminalScreen MainScreen { get; }

    public static TerminalScreen AlternateScreen { get; }

    public static TerminalScreen Screen { get; internal set; }

    static readonly TerminalDriver _driver =
        OperatingSystem.IsWindows() ? WindowsTerminalDriver.Instance :
        OperatingSystem.IsLinux() ? LinuxTerminalDriver.Instance :
        OperatingSystem.IsMacOS() ? MacOSTerminalDriver.Instance :
        throw new TerminalException("This platforms is not supported.");

    static Terminal()
    {
        MainScreen = new(_driver);
        AlternateScreen = new(_driver);
        Screen = MainScreen;

        // Reset all terminal state to sane values.
        _driver.ResetAll();
    }

    public static void GenerateSignal(TerminalSignal signal)
    {
        _driver.GenerateSignal(signal);
    }

    public static void EnableRawMode()
    {
        _driver.EnableRawMode();
    }

    public static void DisableRawMode()
    {
        _driver.DisableRawMode();
    }

    public static void SetMouseEvents(TerminalMouseEvents events)
    {
        _driver.SetMouseEvents(events);
    }

    public static byte? ReadRaw()
    {
        return StdIn.ReadRaw();
    }

    public static string? ReadLine()
    {
        return StdIn.ReadLine();
    }

    public static void Out(ReadOnlySpan<byte> value)
    {
        StdOut.Write(value);
    }

    public static void Out(ReadOnlySpan<char> value)
    {
        StdOut.Write(value);
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

    public static void Error(ReadOnlySpan<byte> value)
    {
        StdError.Write(value);
    }

    public static void Error(ReadOnlySpan<char> value)
    {
        StdError.Write(value);
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

    public static void Beep()
    {
        _driver.Beep();
    }

    public static void Insert(int count = 1)
    {
        _driver.Insert(count);
    }

    public static void Delete(int count = 1)
    {
        _driver.Delete(count);
    }

    public static void Erase(int count = 1)
    {
        _driver.Erase(count);
    }

    public static void InsertLine(int count = 1)
    {
        _driver.InsertLine(count);
    }

    public static void DeleteLine(int count = 1)
    {
        _driver.DeleteLine(count);
    }

    public static void ClearScreen(TerminalClearMode mode = TerminalClearMode.Full)
    {
        _driver.ClearScreen(mode);
    }

    public static void ClearLine(TerminalClearMode mode = TerminalClearMode.Full)
    {
        _driver.ClearLine(mode);
    }

    public static void SetScrollRegion(int top, int bottom)
    {
        _driver.SetScrollRegion(top, bottom);
    }

    public static void MoveBufferUp(int count = 1)
    {
        _driver.MoveBufferUp(count);
    }

    public static void MoveBufferDown(int count = 1)
    {
        _driver.MoveBufferDown(count);
    }

    public static void MoveCursorTo(int row, int column)
    {
        _driver.MoveCursorTo(row, column);
    }

    public static void MoveCursorUp(int count = 1)
    {
        _driver.MoveCursorUp(count);
    }

    public static void MoveCursorDown(int count = 1)
    {
        _driver.MoveCursorDown(count);
    }

    public static void MoveCursorLeft(int count = 1)
    {
        _driver.MoveCursorLeft(count);
    }

    public static void MoveCursorRight(int count = 1)
    {
        _driver.MoveCursorRight(count);
    }

    public static void SaveCursorPosition()
    {
        _driver.SaveCursorPosition();
    }

    public static void RestoreCursorPosition()
    {
        _driver.RestoreCursorPosition();
    }

    public static void ForegroundColor(byte r, byte g, byte b)
    {
        _driver.ForegroundColor(r, g, b);
    }

    public static void BackgroundColor(byte r, byte g, byte b)
    {
        _driver.BackgroundColor(r, g, b);
    }

    public static void Decorations(
        bool bold = false,
        bool faint = false,
        bool italic = false,
        bool underline = false,
        bool blink = false,
        bool invert = false,
        bool invisible = false,
        bool strike = false,
        bool doubleUnderline = false,
        bool overline = false)
    {
        _driver.Decorations(
            bold,
            faint,
            italic,
            underline,
            blink,
            invert,
            invisible,
            strike,
            doubleUnderline,
            overline);
    }

    public static void ResetAttributes()
    {
        _driver.ResetAttributes();
    }

    public static void OpenHyperlink(Uri uri, string? id = null)
    {
        _driver.OpenHyperlink(uri, id);
    }

    public static void CloseHyperlink()
    {
        _driver.CloseHyperlink();
    }
}
