using Cathode.Terminals.Unix.Linux;
using Cathode.Terminals.Unix.MacOS;
using Cathode.Terminals.Windows;

namespace Cathode;

public static class Terminal
{
    public static event Action<TerminalSize>? Resized
    {
        add => System.Resized += value;
        remove => System.Resized -= value;
    }

    public static event Action<TerminalSignalContext>? Signaled
    {
        add => System.Signaled += value;
        remove => System.Signaled -= value;
    }

    public static event Action? Resumed
    {
        add => System.Resumed += value;
        remove => System.Resumed -= value;
    }

    public static Encoding Encoding { get; } = new UTF8Encoding(false);

    public static SystemVirtualTerminal System { get; } =
        OperatingSystem.IsLinux() ? LinuxVirtualTerminal.Instance :
        OperatingSystem.IsMacOS() ? MacOSVirtualTerminal.Instance :
        OperatingSystem.IsWindows() ? WindowsVirtualTerminal.Instance :
        throw new PlatformNotSupportedException();

    public static TerminalControl Control => System.Control;

    public static TerminalReader StandardIn => System.StandardIn;

    public static TerminalWriter StandardOut => System.StandardOut;

    public static TerminalWriter StandardError => System.StandardError;

    public static TerminalReader TerminalIn => System.TerminalIn;

    public static TerminalWriter TerminalOut => System.TerminalOut;

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

    public static int Read(Span<byte> value, CancellationToken cancellationToken = default)
    {
        return System.Read(value, cancellationToken);
    }

    public static ValueTask<int> ReadAsync(Memory<byte> value, CancellationToken cancellationToken = default)
    {
        return System.ReadAsync(value, cancellationToken);
    }

    public static string? ReadLine(CancellationToken cancellationToken = default)
    {
        return System.ReadLine(cancellationToken);
    }

    public static ValueTask<string?> ReadLineAsync(CancellationToken cancellationToken = default)
    {
        return System.ReadLineAsync(cancellationToken);
    }

    public static void Out(ReadOnlySpan<byte> value, CancellationToken cancellationToken = default)
    {
        System.Out(value, cancellationToken);
    }

    public static ValueTask OutAsync(ReadOnlyMemory<byte> value, CancellationToken cancellationToken = default)
    {
        return System.OutAsync(value, cancellationToken);
    }

    public static ValueTask OutAsync(Memory<byte> value, CancellationToken cancellationToken = default)
    {
        return System.OutAsync(value, cancellationToken);
    }

    public static void Out(ReadOnlySpan<char> value, CancellationToken cancellationToken = default)
    {
        System.Out(value, cancellationToken);
    }

    public static ValueTask OutAsync(ReadOnlyMemory<char> value, CancellationToken cancellationToken = default)
    {
        return System.OutAsync(value, cancellationToken);
    }

    public static ValueTask OutAsync(Memory<char> value, CancellationToken cancellationToken = default)
    {
        return System.OutAsync(value, cancellationToken);
    }

    public static void Out<T>(T value, CancellationToken cancellationToken = default)
    {
        System.Out(value, cancellationToken);
    }

    public static ValueTask OutAsync<T>(T value, CancellationToken cancellationToken = default)
    {
        return System.OutAsync(value, cancellationToken);
    }

    public static void OutLine(CancellationToken cancellationToken = default)
    {
        System.OutLine(cancellationToken);
    }

    public static ValueTask OutLineAsync(CancellationToken cancellationToken = default)
    {
        return System.OutLineAsync(cancellationToken);
    }

    public static void OutLine<T>(T value, CancellationToken cancellationToken = default)
    {
        System.OutLine(value, cancellationToken);
    }

    public static ValueTask OutLineAsync<T>(T value, CancellationToken cancellationToken = default)
    {
        return System.OutLineAsync(value, cancellationToken);
    }

    public static void Error(ReadOnlySpan<byte> value, CancellationToken cancellationToken = default)
    {
        System.Error(value, cancellationToken);
    }

    public static ValueTask ErrorAsync(ReadOnlyMemory<byte> value, CancellationToken cancellationToken = default)
    {
        return System.ErrorAsync(value, cancellationToken);
    }

    public static ValueTask ErrorAsync(Memory<byte> value, CancellationToken cancellationToken = default)
    {
        return System.ErrorAsync(value, cancellationToken);
    }

    public static void Error(ReadOnlySpan<char> value, CancellationToken cancellationToken = default)
    {
        System.Error(value, cancellationToken);
    }

    public static ValueTask ErrorAsync(ReadOnlyMemory<char> value, CancellationToken cancellationToken = default)
    {
        return System.ErrorAsync(value, cancellationToken);
    }

    public static ValueTask ErrorAsync(Memory<char> value, CancellationToken cancellationToken = default)
    {
        return System.ErrorAsync(value, cancellationToken);
    }

    public static void Error<T>(T value, CancellationToken cancellationToken = default)
    {
        System.Error(value, cancellationToken);
    }

    public static ValueTask ErrorAsync<T>(T value, CancellationToken cancellationToken = default)
    {
        return System.ErrorAsync(value, cancellationToken);
    }

    public static void ErrorLine(CancellationToken cancellationToken = default)
    {
        System.ErrorLine(cancellationToken);
    }

    public static ValueTask ErrorLineAsync(CancellationToken cancellationToken = default)
    {
        return System.ErrorLineAsync(cancellationToken);
    }

    public static void ErrorLine<T>(T value, CancellationToken cancellationToken = default)
    {
        System.ErrorLine(value, cancellationToken);
    }

    public static ValueTask ErrorLineAsync<T>(T value, CancellationToken cancellationToken = default)
    {
        return System.ErrorLineAsync(value, cancellationToken);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void DangerousRestoreSettings()
    {
        System.DangerousRestoreSettings();
    }
}
