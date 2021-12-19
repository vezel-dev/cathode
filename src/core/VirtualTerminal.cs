namespace System;

public abstract class VirtualTerminal
{
    public abstract event Action<TerminalSize>? Resized;

    public abstract event Action<TerminalSignalContext>? Signaled;

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

    public byte? ReadRaw(CancellationToken cancellationToken = default)
    {
        return StandardIn.ReadRaw(cancellationToken);
    }

    public string? ReadLine(CancellationToken cancellationToken = default)
    {
        return StandardIn.ReadLine(cancellationToken);
    }

    public void Out(ReadOnlySpan<byte> value, CancellationToken cancellationToken = default)
    {
        StandardOut.Write(value, cancellationToken);
    }

    public void Out(ReadOnlySpan<char> value, CancellationToken cancellationToken = default)
    {
        StandardOut.Write(value, cancellationToken);
    }

    public void Out<T>(T value, CancellationToken cancellationToken = default)
    {
        StandardOut.Write(value, cancellationToken);
    }

    public void OutLine(CancellationToken cancellationToken = default)
    {
        StandardOut.WriteLine(cancellationToken);
    }

    public void OutLine<T>(T value, CancellationToken cancellationToken = default)
    {
        StandardOut.WriteLine(value, cancellationToken);
    }

    public void Error(ReadOnlySpan<byte> value, CancellationToken cancellationToken = default)
    {
        StandardError.Write(value, cancellationToken);
    }

    public void Error(ReadOnlySpan<char> value, CancellationToken cancellationToken = default)
    {
        StandardError.Write(value, cancellationToken);
    }

    public void Error<T>(T value, CancellationToken cancellationToken = default)
    {
        StandardError.Write(value, cancellationToken);
    }

    public void ErrorLine(CancellationToken cancellationToken = default)
    {
        StandardError.WriteLine(cancellationToken);
    }

    public void ErrorLine<T>(T value, CancellationToken cancellationToken = default)
    {
        StandardError.WriteLine(value, cancellationToken);
    }
}
