// SPDX-License-Identifier: 0BSD

namespace Vezel.Cathode;

public abstract class VirtualTerminal
{
    public abstract event Action<Size>? Resized;

    public abstract event Action<TerminalSignalContext>? Signaled;

    public abstract event Action? Resumed;

    public abstract TerminalReader StandardIn { get; }

    public abstract TerminalWriter StandardOut { get; }

    public abstract TerminalWriter StandardError { get; }

    public abstract TerminalReader TerminalIn { get; }

    public abstract TerminalWriter TerminalOut { get; }

    public abstract Size Size { get; }

    public abstract bool IsRawMode { get; }

    public abstract void EnableRawMode();

    public abstract void DisableRawMode();

    public abstract void GenerateSignal(TerminalSignal signal);

    public int Read(scoped Span<byte> value)
    {
        return StandardIn.ReadPartial(value);
    }

    public ValueTask<int> ReadAsync(Memory<byte> value, CancellationToken cancellationToken = default)
    {
        return StandardIn.ReadPartialAsync(value, cancellationToken);
    }

    public string? ReadLine()
    {
        return StandardIn.ReadLine();
    }

    public ValueTask<string?> ReadLineAsync(CancellationToken cancellationToken = default)
    {
        return StandardIn.ReadLineAsync(cancellationToken);
    }

    public void Out<T>(T value)
    {
        StandardOut.Write(value);
    }

    public void Out(scoped ReadOnlySpan<byte> value)
    {
        StandardOut.Write(value);
    }

    public void Out(scoped Span<byte> value)
    {
        StandardOut.Write(value);
    }

    public void Out(ReadOnlyMemory<byte> value)
    {
        StandardOut.Write(value);
    }

    public void Out(Memory<byte> value)
    {
        StandardOut.Write(value);
    }

    public void Out(byte[]? value)
    {
        StandardOut.Write(value);
    }

    public void Out(scoped ReadOnlySpan<char> value)
    {
        StandardOut.Write(value);
    }

    public void Out(scoped Span<char> value)
    {
        StandardOut.Write(value);
    }

    public void Out(ReadOnlyMemory<char> value)
    {
        StandardOut.Write(value);
    }

    public void Out(Memory<char> value)
    {
        StandardOut.Write(value);
    }

    public void Out(char[]? value)
    {
        StandardOut.Write(value);
    }

    public void Out(string? value)
    {
        StandardOut.Write(value);
    }

    public ValueTask OutAsync<T>(T value, CancellationToken cancellationToken = default)
    {
        return StandardOut.WriteAsync(value, cancellationToken);
    }

    public ValueTask OutAsync(ReadOnlyMemory<byte> value, CancellationToken cancellationToken = default)
    {
        return StandardOut.WriteAsync(value, cancellationToken);
    }

    public ValueTask OutAsync(Memory<byte> value, CancellationToken cancellationToken = default)
    {
        return StandardOut.WriteAsync(value, cancellationToken);
    }

    public ValueTask OutAsync(byte[]? value, CancellationToken cancellationToken = default)
    {
        return StandardOut.WriteAsync(value, cancellationToken);
    }

    public ValueTask OutAsync(ReadOnlyMemory<char> value, CancellationToken cancellationToken = default)
    {
        return StandardOut.WriteAsync(value, cancellationToken);
    }

    public ValueTask OutAsync(Memory<char> value, CancellationToken cancellationToken = default)
    {
        return StandardOut.WriteAsync(value, cancellationToken);
    }

    public ValueTask OutAsync(char[]? value, CancellationToken cancellationToken = default)
    {
        return StandardOut.WriteAsync(value, cancellationToken);
    }

    public ValueTask OutAsync(string? value, CancellationToken cancellationToken = default)
    {
        return StandardOut.WriteAsync(value, cancellationToken);
    }

    public void OutLine()
    {
        StandardOut.WriteLine();
    }

    public void OutLine<T>(T value)
    {
        StandardOut.WriteLine(value);
    }

    public void OutLine(scoped ReadOnlySpan<byte> value)
    {
        StandardOut.WriteLine(value);
    }

    public void OutLine(scoped Span<byte> value)
    {
        StandardOut.WriteLine(value);
    }

    public void OutLine(ReadOnlyMemory<byte> value)
    {
        StandardOut.WriteLine(value);
    }

    public void OutLine(Memory<byte> value)
    {
        StandardOut.WriteLine(value);
    }

    public void OutLine(byte[]? value)
    {
        StandardOut.WriteLine(value);
    }

    public void OutLine(scoped ReadOnlySpan<char> value)
    {
        StandardOut.WriteLine(value);
    }

    public void OutLine(scoped Span<char> value)
    {
        StandardOut.WriteLine(value);
    }

    public void OutLine(ReadOnlyMemory<char> value)
    {
        StandardOut.WriteLine(value);
    }

    public void OutLine(Memory<char> value)
    {
        StandardOut.WriteLine(value);
    }

    public void OutLine(char[]? value)
    {
        StandardOut.WriteLine(value);
    }

    public void OutLine(string? value)
    {
        StandardOut.WriteLine(value);
    }

    public ValueTask OutLineAsync(CancellationToken cancellationToken = default)
    {
        return StandardOut.WriteLineAsync(cancellationToken);
    }

    public ValueTask OutLineAsync<T>(T value, CancellationToken cancellationToken = default)
    {
        return StandardOut.WriteLineAsync(value, cancellationToken);
    }

    public ValueTask OutLineAsync(ReadOnlyMemory<byte> value, CancellationToken cancellationToken = default)
    {
        return StandardOut.WriteLineAsync(value, cancellationToken);
    }

    public ValueTask OutLineAsync(Memory<byte> value, CancellationToken cancellationToken = default)
    {
        return StandardOut.WriteLineAsync(value, cancellationToken);
    }

    public ValueTask OutLineAsync(byte[]? value, CancellationToken cancellationToken = default)
    {
        return StandardOut.WriteLineAsync(value, cancellationToken);
    }

    public ValueTask OutLineAsync(ReadOnlyMemory<char> value, CancellationToken cancellationToken = default)
    {
        return StandardOut.WriteLineAsync(value, cancellationToken);
    }

    public ValueTask OutLineAsync(Memory<char> value, CancellationToken cancellationToken = default)
    {
        return StandardOut.WriteLineAsync(value, cancellationToken);
    }

    public ValueTask OutLineAsync(char[]? value, CancellationToken cancellationToken = default)
    {
        return StandardOut.WriteLineAsync(value, cancellationToken);
    }

    public ValueTask OutLineAsync(string? value, CancellationToken cancellationToken = default)
    {
        return StandardOut.WriteLineAsync(value, cancellationToken);
    }

    public void Error<T>(T value)
    {
        StandardError.Write(value);
    }

    public void Error(scoped ReadOnlySpan<byte> value)
    {
        StandardError.Write(value);
    }

    public void Error(scoped Span<byte> value)
    {
        StandardError.Write(value);
    }

    public void Error(ReadOnlyMemory<byte> value)
    {
        StandardError.Write(value);
    }

    public void Error(Memory<byte> value)
    {
        StandardError.Write(value);
    }

    public void Error(byte[]? value)
    {
        StandardError.Write(value);
    }

    public void Error(scoped ReadOnlySpan<char> value)
    {
        StandardError.Write(value);
    }

    public void Error(scoped Span<char> value)
    {
        StandardError.Write(value);
    }

    public void Error(ReadOnlyMemory<char> value)
    {
        StandardError.Write(value);
    }

    public void Error(Memory<char> value)
    {
        StandardError.Write(value);
    }

    public void Error(char[]? value)
    {
        StandardError.Write(value);
    }

    public void Error(string? value)
    {
        StandardError.Write(value);
    }

    public ValueTask ErrorAsync<T>(T value, CancellationToken cancellationToken = default)
    {
        return StandardError.WriteAsync(value, cancellationToken);
    }

    public ValueTask ErrorAsync(ReadOnlyMemory<byte> value, CancellationToken cancellationToken = default)
    {
        return StandardError.WriteAsync(value, cancellationToken);
    }

    public ValueTask ErrorAsync(Memory<byte> value, CancellationToken cancellationToken = default)
    {
        return StandardError.WriteAsync(value, cancellationToken);
    }

    public ValueTask ErrorAsync(byte[]? value, CancellationToken cancellationToken = default)
    {
        return StandardError.WriteAsync(value, cancellationToken);
    }

    public ValueTask ErrorAsync(ReadOnlyMemory<char> value, CancellationToken cancellationToken = default)
    {
        return StandardError.WriteAsync(value, cancellationToken);
    }

    public ValueTask ErrorAsync(Memory<char> value, CancellationToken cancellationToken = default)
    {
        return StandardError.WriteAsync(value, cancellationToken);
    }

    public ValueTask ErrorAsync(char[]? value, CancellationToken cancellationToken = default)
    {
        return StandardError.WriteAsync(value, cancellationToken);
    }

    public ValueTask ErrorAsync(string? value, CancellationToken cancellationToken = default)
    {
        return StandardError.WriteAsync(value, cancellationToken);
    }

    public void ErrorLine()
    {
        StandardError.WriteLine();
    }

    public void ErrorLine<T>(T value)
    {
        StandardError.WriteLine(value);
    }

    public void ErrorLine(scoped ReadOnlySpan<byte> value)
    {
        StandardError.WriteLine(value);
    }

    public void ErrorLine(scoped Span<byte> value)
    {
        StandardError.WriteLine(value);
    }

    public void ErrorLine(ReadOnlyMemory<byte> value)
    {
        StandardError.WriteLine(value);
    }

    public void ErrorLine(Memory<byte> value)
    {
        StandardError.WriteLine(value);
    }

    public void ErrorLine(byte[]? value)
    {
        StandardError.WriteLine(value);
    }

    public void ErrorLine(scoped ReadOnlySpan<char> value)
    {
        StandardError.WriteLine(value);
    }

    public void ErrorLine(scoped Span<char> value)
    {
        StandardError.WriteLine(value);
    }

    public void ErrorLine(ReadOnlyMemory<char> value)
    {
        StandardError.WriteLine(value);
    }

    public void ErrorLine(Memory<char> value)
    {
        StandardError.WriteLine(value);
    }

    public void ErrorLine(char[]? value)
    {
        StandardError.WriteLine(value);
    }

    public void ErrorLine(string? value)
    {
        StandardError.WriteLine(value);
    }

    public ValueTask ErrorLineAsync(CancellationToken cancellationToken = default)
    {
        return StandardError.WriteLineAsync(cancellationToken);
    }

    public ValueTask ErrorLineAsync<T>(T value, CancellationToken cancellationToken = default)
    {
        return StandardError.WriteLineAsync(value, cancellationToken);
    }

    public ValueTask ErrorLineAsync(ReadOnlyMemory<byte> value, CancellationToken cancellationToken = default)
    {
        return StandardError.WriteLineAsync(value, cancellationToken);
    }

    public ValueTask ErrorLineAsync(Memory<byte> value, CancellationToken cancellationToken = default)
    {
        return StandardError.WriteLineAsync(value, cancellationToken);
    }

    public ValueTask ErrorLineAsync(byte[]? value, CancellationToken cancellationToken = default)
    {
        return StandardError.WriteLineAsync(value, cancellationToken);
    }

    public ValueTask ErrorLineAsync(ReadOnlyMemory<char> value, CancellationToken cancellationToken = default)
    {
        return StandardError.WriteLineAsync(value, cancellationToken);
    }

    public ValueTask ErrorLineAsync(Memory<char> value, CancellationToken cancellationToken = default)
    {
        return StandardError.WriteLineAsync(value, cancellationToken);
    }

    public ValueTask ErrorLineAsync(char[]? value, CancellationToken cancellationToken = default)
    {
        return StandardError.WriteLineAsync(value, cancellationToken);
    }

    public ValueTask ErrorLineAsync(string? value, CancellationToken cancellationToken = default)
    {
        return StandardError.WriteLineAsync(value, cancellationToken);
    }
}
