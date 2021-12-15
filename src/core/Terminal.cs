namespace System;

public static class Terminal
{
    public static event Action<TerminalSize>? Resized
    {
        add => _terminal.Resized += value;
        remove => _terminal.Resized -= value;
    }

    public static event Action<TerminalSignalContext>? Signaled
    {
        add => _terminal.Signaled += value;
        remove => _terminal.Signaled -= value;
    }

    public static Encoding Encoding { get; } = new UTF8Encoding(false);

    public static TerminalReader StandardIn => _terminal.StandardIn;

    public static TerminalWriter StandardOut => _terminal.StandardOut;

    public static TerminalWriter StandardError => _terminal.StandardError;

    public static TerminalReader TerminalIn => _terminal.TerminalIn;

    public static TerminalWriter TerminalOut => _terminal.TerminalOut;

    public static bool IsRawMode => _terminal.IsRawMode;

    public static TerminalSize Size => _terminal.Size;

    public static TimeSpan SizePollingInterval
    {
        get => _terminal.SizePollingInterval;
        set => _terminal.SizePollingInterval = value;
    }

    static readonly SystemVirtualTerminal _terminal = SystemVirtualTerminal.Instance;

    public static void GenerateSignal(TerminalSignal signal)
    {
        _terminal.GenerateSignal(signal);
    }

    public static void EnableRawMode()
    {
        _terminal.EnableRawMode();
    }

    public static void DisableRawMode()
    {
        _terminal.DisableRawMode();
    }

    public static byte? ReadRaw()
    {
        return _terminal.ReadRaw();
    }

    public static string? ReadLine()
    {
        return _terminal.ReadLine();
    }

    public static void Out(ReadOnlySpan<byte> value)
    {
        _terminal.Out(value);
    }

    public static void Out(ReadOnlySpan<char> value)
    {
        _terminal.Out(value);
    }

    public static void Out(string? value)
    {
        _terminal.Out(value);
    }

    public static void Out<T>(T value)
    {
        _terminal.Out(value);
    }

    public static void Out(string format, params object?[] args)
    {
        _terminal.Out(format, args);
    }

    public static void OutLine()
    {
        _terminal.OutLine();
    }

    public static void OutLine(string? value)
    {
        _terminal.OutLine(value);
    }

    public static void OutLine<T>(T value)
    {
        _terminal.OutLine(value);
    }

    public static void OutLine(string format, params object?[] args)
    {
        _terminal.OutLine(format, args);
    }

    public static void Error(ReadOnlySpan<byte> value)
    {
        _terminal.Error(value);
    }

    public static void Error(ReadOnlySpan<char> value)
    {
        _terminal.Error(value);
    }

    public static void Error(string? value)
    {
        _terminal.Error(value);
    }

    public static void Error<T>(T value)
    {
        _terminal.Error(value);
    }

    public static void Error(string format, params object?[] args)
    {
        _terminal.Error(format, args);
    }

    public static void ErrorLine()
    {
        _terminal.ErrorLine();
    }

    public static void ErrorLine(string? value)
    {
        _terminal.ErrorLine(value);
    }

    public static void ErrorLine<T>(T value)
    {
        _terminal.ErrorLine(value);
    }

    public static void ErrorLine(string format, params object?[] args)
    {
        _terminal.ErrorLine(format, args);
    }
}
