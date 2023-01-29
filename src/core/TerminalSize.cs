namespace Vezel.Cathode;

public readonly struct TerminalSize : IEquatable<TerminalSize>, IEqualityOperators<TerminalSize, TerminalSize, bool>
{
    public int Width { get; }

    public int Height { get; }

    public TerminalSize(int width, int height)
    {
        Check.Range(width >= 0, width);
        Check.Range(height >= 0, height);

        Width = width;
        Height = height;
    }

    public static bool operator ==(TerminalSize left, TerminalSize right) => left.Equals(right);

    public static bool operator !=(TerminalSize left, TerminalSize right) => !left.Equals(right);

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

    public override string ToString()
    {
        return $"{Width}x{Height}";
    }
}
