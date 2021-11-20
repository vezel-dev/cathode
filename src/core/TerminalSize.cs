namespace System;

[Serializable]
public readonly struct TerminalSize : IEquatable<TerminalSize>
{
    public int Width { get; }

    public int Height { get; }

    internal TerminalSize(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public static bool operator ==(TerminalSize left, TerminalSize right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(TerminalSize left, TerminalSize right)
    {
        return !left.Equals(right);
    }

    public bool Equals(TerminalSize other)
    {
        return Width == other.Width && Height == other.Height;
    }

    public override bool Equals(object? obj)
    {
        return obj is TerminalSize s && Equals(s);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Width, Height);
    }
}
