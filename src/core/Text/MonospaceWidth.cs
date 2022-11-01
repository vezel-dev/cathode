using Wcwidth;

namespace Vezel.Cathode.Text;

public static class MonospaceWidth
{
    private const Unicode Version = Unicode.Version_15_0_0;

    public static int? Measure(Rune value)
    {
        var width = UnicodeCalculator.GetWidth(value, Version);

        return width == -1 ? null : width;
    }

    public static int? Measure(scoped ReadOnlySpan<char> value)
    {
        var result = 0;

        foreach (var rune in value.EnumerateRunes())
        {
            if (Measure(rune) is int i)
                result += i;
            else
                return null;
        }

        return result;
    }

    public static int? Measure(string? value)
    {
        return Measure(value.AsSpan());
    }
}
