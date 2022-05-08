namespace Vezel.Cathode;

public readonly struct TerminalSize : IEquatable<TerminalSize>
{
    public int Width { get; }

    public int Height { get; }

    public TerminalSize(int width, int height)
    {
        _ = width >= 0 ? true : throw new ArgumentOutOfRangeException(nameof(width));
        _ = height >= 0 ? true : throw new ArgumentOutOfRangeException(nameof(height));

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

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is TerminalSize s && Equals(s);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Width, Height);
    }
}
