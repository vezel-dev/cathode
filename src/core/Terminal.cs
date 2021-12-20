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

    public static event Action? Resumed
    {
        add => _terminal.Resumed += value;
        remove => _terminal.Resumed -= value;
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

    public static ValueTask<byte?> ReadRawAsync(CancellationToken cancellationToken = default)
    {
        return _terminal.ReadRawAsync(cancellationToken);
    }

    public static string? ReadLine(CancellationToken cancellationToken = default)
    {
        return _terminal.ReadLine(cancellationToken);
    }

    public static ValueTask<string?> ReadLineAsync(CancellationToken cancellationToken = default)
    {
        return _terminal.ReadLineAsync(cancellationToken);
    }

    public static void Out(ReadOnlySpan<byte> value, CancellationToken cancellationToken = default)
    {
        _terminal.Out(value, cancellationToken);
    }

    public static ValueTask OutAsync(ReadOnlyMemory<byte> value, CancellationToken cancellationToken = default)
    {
        return _terminal.OutAsync(value, cancellationToken);
    }

    public static ValueTask OutAsync(Memory<byte> value, CancellationToken cancellationToken = default)
    {
        return _terminal.OutAsync(value, cancellationToken);
    }

    public static void Out(ReadOnlySpan<char> value, CancellationToken cancellationToken = default)
    {
        _terminal.Out(value, cancellationToken);
    }

    public static ValueTask OutAsync(ReadOnlyMemory<char> value, CancellationToken cancellationToken = default)
    {
        return _terminal.OutAsync(value, cancellationToken);
    }

    public static ValueTask OutAsync(Memory<char> value, CancellationToken cancellationToken = default)
    {
        return _terminal.OutAsync(value, cancellationToken);
    }

    public static void Out<T>(T value, CancellationToken cancellationToken = default)
    {
        _terminal.Out(value, cancellationToken);
    }

    public static ValueTask OutAsync<T>(T value, CancellationToken cancellationToken = default)
    {
        return _terminal.OutAsync(value, cancellationToken);
    }

    public static void OutLine(CancellationToken cancellationToken = default)
    {
        _terminal.OutLine(cancellationToken);
    }

    public static ValueTask OutLineAsync(CancellationToken cancellationToken = default)
    {
        return _terminal.OutLineAsync(cancellationToken);
    }

    public static void OutLine<T>(T value, CancellationToken cancellationToken = default)
    {
        _terminal.OutLine(value, cancellationToken);
    }

    public static ValueTask OutLineAsync<T>(T value, CancellationToken cancellationToken = default)
    {
        return _terminal.OutLineAsync(value, cancellationToken);
    }

    public static void Error(ReadOnlySpan<byte> value, CancellationToken cancellationToken = default)
    {
        _terminal.Error(value, cancellationToken);
    }

    public static ValueTask ErrorAsync(ReadOnlyMemory<byte> value, CancellationToken cancellationToken = default)
    {
        return _terminal.ErrorAsync(value, cancellationToken);
    }

    public static ValueTask ErrorAsync(Memory<byte> value, CancellationToken cancellationToken = default)
    {
        return _terminal.ErrorAsync(value, cancellationToken);
    }

    public static void Error(ReadOnlySpan<char> value, CancellationToken cancellationToken = default)
    {
        _terminal.Error(value, cancellationToken);
    }

    public static ValueTask ErrorAsync(ReadOnlyMemory<char> value, CancellationToken cancellationToken = default)
    {
        return _terminal.ErrorAsync(value, cancellationToken);
    }

    public static ValueTask ErrorAsync(Memory<char> value, CancellationToken cancellationToken = default)
    {
        return _terminal.ErrorAsync(value, cancellationToken);
    }

    public static void Error<T>(T value, CancellationToken cancellationToken = default)
    {
        _terminal.Error(value, cancellationToken);
    }

    public static ValueTask ErrorAsync<T>(T value, CancellationToken cancellationToken = default)
    {
        return _terminal.ErrorAsync(value, cancellationToken);
    }

    public static void ErrorLine(CancellationToken cancellationToken = default)
    {
        _terminal.ErrorLine(cancellationToken);
    }

    public static ValueTask ErrorLineAsync(CancellationToken cancellationToken = default)
    {
        return _terminal.ErrorLineAsync(cancellationToken);
    }

    public static void ErrorLine<T>(T value, CancellationToken cancellationToken = default)
    {
        _terminal.ErrorLine(value, cancellationToken);
    }

    public static ValueTask ErrorLineAsync<T>(T value, CancellationToken cancellationToken = default)
    {
        return _terminal.ErrorLineAsync(value, cancellationToken);
    }
}
