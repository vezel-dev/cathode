namespace System;

public static class Terminal
{
    public static event Action<TerminalSize>? Resize
    {
        add => System.Resize += value;
        remove => System.Resize -= value;
    }

    public static event Action<TerminalSignalContext>? Signal
    {
        add => System.Signal += value;
        remove => System.Signal -= value;
    }

    public static Encoding Encoding { get; } = new UTF8Encoding(false);

    public static SystemVirtualTerminal System => SystemVirtualTerminal.Instance;

    public static TerminalReader StdIn => System.StdIn;

    public static TerminalWriter StdOut => System.StdOut;

    public static TerminalWriter StdError => System.StdError;

    public static bool IsRawMode => System.IsRawMode;

    public static TerminalSize Size => System.Size;

    public static TimeSpan SizePollingInterval
    {
        get => System.SizePollingInterval;
        set => System.SizePollingInterval = value;
    }

    public static void GenerateSignal(TerminalSignal signal)
    {
        System.GenerateSignal(signal);
    }

    public static void EnableRawMode()
    {
        System.EnableRawMode();
    }

    public static void DisableRawMode()
    {
        System.DisableRawMode();
    }

    public static byte? ReadRaw()
    {
        return System.ReadRaw();
    }

    public static string? ReadLine()
    {
        return System.ReadLine();
    }

    public static void Out(ReadOnlySpan<byte> value)
    {
        System.Out(value);
    }

    public static void Out(ReadOnlySpan<char> value)
    {
        System.Out(value);
    }

    public static void Out(string? value)
    {
        System.Out(value);
    }

    public static void Out<T>(T value)
    {
        System.Out(value);
    }

    public static void Out(string format, params object?[] args)
    {
        System.Out(format, args);
    }

    public static void OutLine()
    {
        System.OutLine();
    }

    public static void OutLine(string? value)
    {
        System.OutLine(value);
    }

    public static void OutLine<T>(T value)
    {
        System.OutLine(value);
    }

    public static void OutLine(string format, params object?[] args)
    {
        System.OutLine(format, args);
    }

    public static void Error(ReadOnlySpan<byte> value)
    {
        System.Error(value);
    }

    public static void Error(ReadOnlySpan<char> value)
    {
        System.Error(value);
    }

    public static void Error(string? value)
    {
        System.Error(value);
    }

    public static void Error<T>(T value)
    {
        System.Error(value);
    }

    public static void Error(string format, params object?[] args)
    {
        System.Error(format, args);
    }

    public static void ErrorLine()
    {
        System.ErrorLine();
    }

    public static void ErrorLine(string? value)
    {
        System.ErrorLine(value);
    }

    public static void ErrorLine<T>(T value)
    {
        System.ErrorLine(value);
    }

    public static void ErrorLine(string format, params object?[] args)
    {
        System.ErrorLine(format, args);
    }
}
