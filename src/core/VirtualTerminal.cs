namespace Vezel.Cathode;

public abstract class VirtualTerminal
{
    public abstract event Action<TerminalSize>? Resized;

    public abstract event Action<TerminalSignalContext>? Signaled;

    public abstract event Action? Resumed;

    public abstract TerminalReader StandardIn { get; }

    public abstract TerminalWriter StandardOut { get; }

    public abstract TerminalWriter StandardError { get; }

    public abstract TerminalReader TerminalIn { get; }

    public abstract TerminalWriter TerminalOut { get; }

    public abstract bool IsRawMode { get; }

    public abstract TerminalSize Size { get; }

    public abstract void GenerateSignal(TerminalSignal signal);

    public abstract void EnableRawMode();

    public abstract void DisableRawMode();

    public int Read(scoped Span<byte> value, CancellationToken cancellationToken = default)
    {
        return StandardIn.ReadPartial(value, cancellationToken);
    }

    public ValueTask<int> ReadAsync(Memory<byte> value, CancellationToken cancellationToken = default)
    {
        return StandardIn.ReadPartialAsync(value, cancellationToken);
    }

    public string? ReadLine(CancellationToken cancellationToken = default)
    {
        return StandardIn.ReadLine(cancellationToken);
    }

    public ValueTask<string?> ReadLineAsync(CancellationToken cancellationToken = default)
    {
        return StandardIn.ReadLineAsync(cancellationToken);
    }

    public void Out(scoped ReadOnlySpan<byte> value, CancellationToken cancellationToken = default)
    {
        StandardOut.Write(value, cancellationToken);
    }

    public ValueTask OutAsync(ReadOnlyMemory<byte> value, CancellationToken cancellationToken = default)
    {
        return StandardOut.WriteAsync(value, cancellationToken);
    }

    public ValueTask OutAsync(Memory<byte> value, CancellationToken cancellationToken = default)
    {
        return StandardOut.WriteAsync(value, cancellationToken);
    }

    public void Out(scoped ReadOnlySpan<char> value, CancellationToken cancellationToken = default)
    {
        StandardOut.Write(value, cancellationToken);
    }

    public ValueTask OutAsync(ReadOnlyMemory<char> value, CancellationToken cancellationToken = default)
    {
        return StandardOut.WriteAsync(value, cancellationToken);
    }

    public ValueTask OutAsync(Memory<char> value, CancellationToken cancellationToken = default)
    {
        return StandardOut.WriteAsync(value, cancellationToken);
    }

    public void Out<T>(T value, CancellationToken cancellationToken = default)
    {
        StandardOut.Write(value, cancellationToken);
    }

    public ValueTask OutAsync<T>(T value, CancellationToken cancellationToken = default)
    {
        return StandardOut.WriteAsync(value, cancellationToken);
    }

    public void OutLine(CancellationToken cancellationToken = default)
    {
        StandardOut.WriteLine(cancellationToken);
    }

    public ValueTask OutLineAsync(CancellationToken cancellationToken = default)
    {
        return StandardOut.WriteLineAsync(cancellationToken);
    }

    public void OutLine<T>(T value, CancellationToken cancellationToken = default)
    {
        StandardOut.WriteLine(value, cancellationToken);
    }

    public ValueTask OutLineAsync<T>(T value, CancellationToken cancellationToken = default)
    {
        return StandardOut.WriteLineAsync(value, cancellationToken);
    }

    public void Error(scoped ReadOnlySpan<byte> value, CancellationToken cancellationToken = default)
    {
        StandardError.Write(value, cancellationToken);
    }

    public ValueTask ErrorAsync(ReadOnlyMemory<byte> value, CancellationToken cancellationToken = default)
    {
        return StandardError.WriteAsync(value, cancellationToken);
    }

    public ValueTask ErrorAsync(Memory<byte> value, CancellationToken cancellationToken = default)
    {
        return StandardError.WriteAsync(value, cancellationToken);
    }

    public void Error(scoped ReadOnlySpan<char> value, CancellationToken cancellationToken = default)
    {
        StandardError.Write(value, cancellationToken);
    }

    public ValueTask ErrorAsync(ReadOnlyMemory<char> value, CancellationToken cancellationToken = default)
    {
        return StandardError.WriteAsync(value, cancellationToken);
    }

    public ValueTask ErrorAsync(Memory<char> value, CancellationToken cancellationToken = default)
    {
        return StandardError.WriteAsync(value, cancellationToken);
    }

    public void Error<T>(T value, CancellationToken cancellationToken = default)
    {
        StandardError.Write(value, cancellationToken);
    }

    public ValueTask ErrorAsync<T>(T value, CancellationToken cancellationToken = default)
    {
        return StandardError.WriteAsync(value, cancellationToken);
    }

    public void ErrorLine(CancellationToken cancellationToken = default)
    {
        StandardError.WriteLine(cancellationToken);
    }

    public ValueTask ErrorLineAsync(CancellationToken cancellationToken = default)
    {
        return StandardError.WriteLineAsync(cancellationToken);
    }

    public void ErrorLine<T>(T value, CancellationToken cancellationToken = default)
    {
        StandardError.WriteLine(value, cancellationToken);
    }

    public ValueTask ErrorLineAsync<T>(T value, CancellationToken cancellationToken = default)
    {
        return StandardError.WriteLineAsync(value, cancellationToken);
    }
}
