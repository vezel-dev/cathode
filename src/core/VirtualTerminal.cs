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

    public byte? ReadRaw()
    {
        return StandardIn.ReadRaw();
    }

    public string? ReadLine()
    {
        return StandardIn.ReadLine();
    }

    public void Out(ReadOnlySpan<byte> value)
    {
        StandardOut.Write(value);
    }

    public void Out(ReadOnlySpan<char> value)
    {
        StandardOut.Write(value);
    }

    public void Out(string? value)
    {
        StandardOut.Write(value);
    }

    public void Out<T>(T value)
    {
        StandardOut.Write(value);
    }

    public void Out(string format, params object?[] args)
    {
        StandardOut.Write(format, args);
    }

    public void OutLine()
    {
        StandardOut.WriteLine();
    }

    public void OutLine(string? value)
    {
        StandardOut.WriteLine(value);
    }

    public void OutLine<T>(T value)
    {
        StandardOut.WriteLine(value);
    }

    public void OutLine(string format, params object?[] args)
    {
        StandardOut.WriteLine(format, args);
    }

    public void Error(ReadOnlySpan<byte> value)
    {
        StandardError.Write(value);
    }

    public void Error(ReadOnlySpan<char> value)
    {
        StandardError.Write(value);
    }

    public void Error(string? value)
    {
        StandardError.Write(value);
    }

    public void Error<T>(T value)
    {
        StandardError.Write(value);
    }

    public void Error(string format, params object?[] args)
    {
        StandardError.Write(format, args);
    }

    public void ErrorLine()
    {
        StandardError.WriteLine();
    }

    public void ErrorLine(string? value)
    {
        StandardError.WriteLine(value);
    }

    public void ErrorLine<T>(T value)
    {
        StandardError.WriteLine(value);
    }

    public void ErrorLine(string format, params object?[] args)
    {
        StandardError.WriteLine(format, args);
    }
}
