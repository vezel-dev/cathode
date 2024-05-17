// SPDX-License-Identifier: 0BSD

using Wcwidth;

namespace Vezel.Cathode.Text;

public static class MonospaceWidth
{
    public static int? Measure(Rune value)
    {
        var width = UnicodeCalculator.GetWidth(value);

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
}
