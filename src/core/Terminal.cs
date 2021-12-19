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

    public static byte? ReadRaw(CancellationToken cancellationToken = default)
    {
        return _terminal.ReadRaw(cancellationToken);
    }

    public static string? ReadLine(CancellationToken cancellationToken = default)
    {
        return _terminal.ReadLine(cancellationToken);
    }

    public static void Out(ReadOnlySpan<byte> value, CancellationToken cancellationToken = default)
    {
        _terminal.Out(value, cancellationToken);
    }

    public static void Out(ReadOnlySpan<char> value, CancellationToken cancellationToken = default)
    {
        _terminal.Out(value, cancellationToken);
    }

    public static void Out<T>(T value, CancellationToken cancellationToken = default)
    {
        _terminal.Out(value, cancellationToken);
    }

    public static void OutLine(CancellationToken cancellationToken = default)
    {
        _terminal.OutLine(cancellationToken);
    }

    public static void OutLine<T>(T value, CancellationToken cancellationToken = default)
    {
        _terminal.OutLine(value, cancellationToken);
    }

    public static void Error(ReadOnlySpan<byte> value, CancellationToken cancellationToken = default)
    {
        _terminal.Error(value, cancellationToken);
    }

    public static void Error(ReadOnlySpan<char> value, CancellationToken cancellationToken = default)
    {
        _terminal.Error(value, cancellationToken);
    }

    public static void Error<T>(T value, CancellationToken cancellationToken = default)
    {
        _terminal.Error(value, cancellationToken);
    }

    public static void ErrorLine(CancellationToken cancellationToken = default)
    {
        _terminal.ErrorLine(cancellationToken);
    }

    public static void ErrorLine<T>(T value, CancellationToken cancellationToken = default)
    {
        _terminal.ErrorLine(value, cancellationToken);
    }
}
