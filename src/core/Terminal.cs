// SPDX-License-Identifier: 0BSD

using Vezel.Cathode.Terminals;

namespace Vezel.Cathode;

public static class Terminal
{
    public static event Action<Size>? Resized
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

    public static Encoding Encoding { get; } = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    public static SystemVirtualTerminal System { get; } =
        OperatingSystem.IsWindows()
            ? WindowsVirtualTerminal.Instance
            : UnixVirtualTerminal.Instance;

    public static TerminalControl Control => System.Control;

    public static TerminalReader StandardIn => System.StandardIn;

    public static TerminalWriter StandardOut => System.StandardOut;

    public static TerminalWriter StandardError => System.StandardError;

    public static TerminalReader TerminalIn => System.TerminalIn;

    public static TerminalWriter TerminalOut => System.TerminalOut;

    public static bool IsRawMode => System.IsRawMode;

    public static Size Size => System.Size;

    public static TimeSpan SizePollingInterval
    {
        get => System.SizePollingInterval;
        set => System.SizePollingInterval = value;
    }

    [UnsupportedOSPlatform("windows")]
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

    public static int Read(scoped Span<byte> value)
    {
        return System.Read(value);
    }

    public static ValueTask<int> ReadAsync(Memory<byte> value, CancellationToken cancellationToken = default)
    {
        return System.ReadAsync(value, cancellationToken);
    }

    public static string? ReadLine()
    {
        return System.ReadLine();
    }

    public static ValueTask<string?> ReadLineAsync(CancellationToken cancellationToken = default)
    {
        return System.ReadLineAsync(cancellationToken);
    }

    public static void Out<T>(T value)
    {
        System.Out(value);
    }

    public static void Out(scoped ReadOnlySpan<byte> value)
    {
        System.Out(value);
    }

    public static void Out(scoped Span<byte> value)
    {
        System.Out(value);
    }

    public static void Out(ReadOnlyMemory<byte> value)
    {
        System.Out(value);
    }

    public static void Out(Memory<byte> value)
    {
        System.Out(value);
    }

    public static void Out(byte[]? value)
    {
        System.Out(value);
    }

    public static void Out(scoped ReadOnlySpan<char> value)
    {
        System.Out(value);
    }

    public static void Out(scoped Span<char> value)
    {
        System.Out(value);
    }

    public static void Out(ReadOnlyMemory<char> value)
    {
        System.Out(value);
    }

    public static void Out(Memory<char> value)
    {
        System.Out(value);
    }

    public static void Out(char[]? value)
    {
        System.Out(value);
    }

    public static void Out(string? value)
    {
        System.Out(value);
    }

    public static ValueTask OutAsync<T>(T value, CancellationToken cancellationToken = default)
    {
        return System.OutAsync(value, cancellationToken);
    }

    public static ValueTask OutAsync(ReadOnlyMemory<byte> value, CancellationToken cancellationToken = default)
    {
        return System.OutAsync(value, cancellationToken);
    }

    public static ValueTask OutAsync(Memory<byte> value, CancellationToken cancellationToken = default)
    {
        return System.OutAsync(value, cancellationToken);
    }

    public static ValueTask OutAsync(byte[]? value, CancellationToken cancellationToken = default)
    {
        return System.OutAsync(value, cancellationToken);
    }

    public static ValueTask OutAsync(ReadOnlyMemory<char> value, CancellationToken cancellationToken = default)
    {
        return System.OutAsync(value, cancellationToken);
    }

    public static ValueTask OutAsync(Memory<char> value, CancellationToken cancellationToken = default)
    {
        return System.OutAsync(value, cancellationToken);
    }

    public static ValueTask OutAsync(char[]? value, CancellationToken cancellationToken = default)
    {
        return System.OutAsync(value, cancellationToken);
    }

    public static ValueTask OutAsync(string? value, CancellationToken cancellationToken = default)
    {
        return System.OutAsync(value, cancellationToken);
    }

    public static void OutLine()
    {
        System.OutLine();
    }

    public static void OutLine<T>(T value)
    {
        System.OutLine(value);
    }

    public static void OutLine(scoped ReadOnlySpan<byte> value)
    {
        System.OutLine(value);
    }

    public static void OutLine(scoped Span<byte> value)
    {
        System.OutLine(value);
    }

    public static void OutLine(ReadOnlyMemory<byte> value)
    {
        System.OutLine(value);
    }

    public static void OutLine(Memory<byte> value)
    {
        System.OutLine(value);
    }

    public static void OutLine(byte[]? value)
    {
        System.OutLine(value);
    }

    public static void OutLine(scoped ReadOnlySpan<char> value)
    {
        System.OutLine(value);
    }

    public static void OutLine(scoped Span<char> value)
    {
        System.OutLine(value);
    }

    public static void OutLine(ReadOnlyMemory<char> value)
    {
        System.OutLine(value);
    }

    public static void OutLine(Memory<char> value)
    {
        System.OutLine(value);
    }

    public static void OutLine(char[]? value)
    {
        System.OutLine(value);
    }

    public static void OutLine(string? value)
    {
        System.OutLine(value);
    }

    public static ValueTask OutLineAsync(CancellationToken cancellationToken = default)
    {
        return System.OutLineAsync(cancellationToken);
    }

    public static ValueTask OutLineAsync<T>(T value, CancellationToken cancellationToken = default)
    {
        return System.OutLineAsync(value, cancellationToken);
    }

    public static ValueTask OutLineAsync(ReadOnlyMemory<byte> value, CancellationToken cancellationToken = default)
    {
        return System.OutLineAsync(value, cancellationToken);
    }

    public static ValueTask OutLineAsync(Memory<byte> value, CancellationToken cancellationToken = default)
    {
        return System.OutLineAsync(value, cancellationToken);
    }

    public static ValueTask OutLineAsync(byte[]? value, CancellationToken cancellationToken = default)
    {
        return System.OutLineAsync(value, cancellationToken);
    }

    public static ValueTask OutLineAsync(ReadOnlyMemory<char> value, CancellationToken cancellationToken = default)
    {
        return System.OutLineAsync(value, cancellationToken);
    }

    public static ValueTask OutLineAsync(Memory<char> value, CancellationToken cancellationToken = default)
    {
        return System.OutLineAsync(value, cancellationToken);
    }

    public static ValueTask OutLineAsync(char[]? value, CancellationToken cancellationToken = default)
    {
        return System.OutLineAsync(value, cancellationToken);
    }

    public static ValueTask OutLineAsync(string? value, CancellationToken cancellationToken = default)
    {
        return System.OutLineAsync(value, cancellationToken);
    }

    public static void Error<T>(T value)
    {
        System.Error(value);
    }

    public static void Error(scoped ReadOnlySpan<byte> value)
    {
        System.Error(value);
    }

    public static void Error(scoped Span<byte> value)
    {
        System.Error(value);
    }

    public static void Error(ReadOnlyMemory<byte> value)
    {
        System.Error(value);
    }

    public static void Error(Memory<byte> value)
    {
        System.Error(value);
    }

    public static void Error(byte[]? value)
    {
        System.Error(value);
    }

    public static void Error(scoped ReadOnlySpan<char> value)
    {
        System.Error(value);
    }

    public static void Error(scoped Span<char> value)
    {
        System.Error(value);
    }

    public static void Error(ReadOnlyMemory<char> value)
    {
        System.Error(value);
    }

    public static void Error(Memory<char> value)
    {
        System.Error(value);
    }

    public static void Error(char[]? value)
    {
        System.Error(value);
    }

    public static void Error(string? value)
    {
        System.Error(value);
    }

    public static ValueTask ErrorAsync<T>(T value, CancellationToken cancellationToken = default)
    {
        return System.ErrorAsync(value, cancellationToken);
    }

    public static ValueTask ErrorAsync(ReadOnlyMemory<byte> value, CancellationToken cancellationToken = default)
    {
        return System.ErrorAsync(value, cancellationToken);
    }

    public static ValueTask ErrorAsync(Memory<byte> value, CancellationToken cancellationToken = default)
    {
        return System.ErrorAsync(value, cancellationToken);
    }

    public static ValueTask ErrorAsync(byte[]? value, CancellationToken cancellationToken = default)
    {
        return System.ErrorAsync(value, cancellationToken);
    }

    public static ValueTask ErrorAsync(ReadOnlyMemory<char> value, CancellationToken cancellationToken = default)
    {
        return System.ErrorAsync(value, cancellationToken);
    }

    public static ValueTask ErrorAsync(Memory<char> value, CancellationToken cancellationToken = default)
    {
        return System.ErrorAsync(value, cancellationToken);
    }

    public static ValueTask ErrorAsync(char[]? value, CancellationToken cancellationToken = default)
    {
        return System.ErrorAsync(value, cancellationToken);
    }

    public static ValueTask ErrorAsync(string? value, CancellationToken cancellationToken = default)
    {
        return System.ErrorAsync(value, cancellationToken);
    }

    public static void ErrorLine()
    {
        System.ErrorLine();
    }

    public static void ErrorLine<T>(T value)
    {
        System.ErrorLine(value);
    }

    public static void ErrorLine(scoped ReadOnlySpan<byte> value)
    {
        System.ErrorLine(value);
    }

    public static void ErrorLine(scoped Span<byte> value)
    {
        System.ErrorLine(value);
    }

    public static void ErrorLine(ReadOnlyMemory<byte> value)
    {
        System.ErrorLine(value);
    }

    public static void ErrorLine(Memory<byte> value)
    {
        System.ErrorLine(value);
    }

    public static void ErrorLine(byte[]? value)
    {
        System.ErrorLine(value);
    }

    public static void ErrorLine(scoped ReadOnlySpan<char> value)
    {
        System.ErrorLine(value);
    }

    public static void ErrorLine(scoped Span<char> value)
    {
        System.ErrorLine(value);
    }

    public static void ErrorLine(ReadOnlyMemory<char> value)
    {
        System.ErrorLine(value);
    }

    public static void ErrorLine(Memory<char> value)
    {
        System.ErrorLine(value);
    }

    public static void ErrorLine(char[]? value)
    {
        System.ErrorLine(value);
    }

    public static void ErrorLine(string? value)
    {
        System.ErrorLine(value);
    }

    public static ValueTask ErrorLineAsync(CancellationToken cancellationToken = default)
    {
        return System.ErrorLineAsync(cancellationToken);
    }

    public static ValueTask ErrorLineAsync<T>(T value, CancellationToken cancellationToken = default)
    {
        return System.ErrorLineAsync(value, cancellationToken);
    }

    public static ValueTask ErrorLineAsync(ReadOnlyMemory<byte> value, CancellationToken cancellationToken = default)
    {
        return System.ErrorLineAsync(value, cancellationToken);
    }

    public static ValueTask ErrorLineAsync(Memory<byte> value, CancellationToken cancellationToken = default)
    {
        return System.ErrorLineAsync(value, cancellationToken);
    }

    public static ValueTask ErrorLineAsync(byte[]? value, CancellationToken cancellationToken = default)
    {
        return System.ErrorLineAsync(value, cancellationToken);
    }

    public static ValueTask ErrorLineAsync(ReadOnlyMemory<char> value, CancellationToken cancellationToken = default)
    {
        return System.ErrorLineAsync(value, cancellationToken);
    }

    public static ValueTask ErrorLineAsync(Memory<char> value, CancellationToken cancellationToken = default)
    {
        return System.ErrorLineAsync(value, cancellationToken);
    }

    public static ValueTask ErrorLineAsync(char[]? value, CancellationToken cancellationToken = default)
    {
        return System.ErrorLineAsync(value, cancellationToken);
    }

    public static ValueTask ErrorLineAsync(string? value, CancellationToken cancellationToken = default)
    {
        return System.ErrorLineAsync(value, cancellationToken);
    }
}
