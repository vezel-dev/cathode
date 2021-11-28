namespace System;

public abstract class VirtualTerminal
{
    public abstract event Action<TerminalSize>? Resize;

    public abstract event Action<TerminalSignalContext>? Signal;

    public abstract TerminalReader StdIn { get; }

    public abstract TerminalWriter StdOut { get; }

    public abstract TerminalWriter StdError { get; }

    public abstract bool IsRawMode { get; }

    public abstract TerminalSize Size { get; }

    public abstract void GenerateSignal(TerminalSignal signal);

    public abstract void EnableRawMode();

    public abstract void DisableRawMode();

    public byte? ReadRaw()
    {
        return StdIn.ReadRaw();
    }

    public string? ReadLine()
    {
        return StdIn.ReadLine();
    }

    public void Out(ReadOnlySpan<byte> value)
    {
        StdOut.Write(value);
    }

    public void Out(ReadOnlySpan<char> value)
    {
        StdOut.Write(value);
    }

    public void Out(string? value)
    {
        StdOut.Write(value);
    }

    public void Out<T>(T value)
    {
        StdOut.Write(value);
    }

    public void Out(string format, params object?[] args)
    {
        StdOut.Write(format, args);
    }

    public void OutLine()
    {
        StdOut.WriteLine();
    }

    public void OutLine(string? value)
    {
        StdOut.WriteLine(value);
    }

    public void OutLine<T>(T value)
    {
        StdOut.WriteLine(value);
    }

    public void OutLine(string format, params object?[] args)
    {
        StdOut.WriteLine(format, args);
    }

    public void Error(ReadOnlySpan<byte> value)
    {
        StdError.Write(value);
    }

    public void Error(ReadOnlySpan<char> value)
    {
        StdError.Write(value);
    }

    public void Error(string? value)
    {
        StdError.Write(value);
    }

    public void Error<T>(T value)
    {
        StdError.Write(value);
    }

    public void Error(string format, params object?[] args)
    {
        StdError.Write(format, args);
    }

    public void ErrorLine()
    {
        StdError.WriteLine();
    }

    public void ErrorLine(string? value)
    {
        StdError.WriteLine(value);
    }

    public void ErrorLine<T>(T value)
    {
        StdError.WriteLine(value);
    }

    public void ErrorLine(string format, params object?[] args)
    {
        StdError.WriteLine(format, args);
    }
}
