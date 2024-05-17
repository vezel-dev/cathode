// SPDX-License-Identifier: 0BSD

namespace Vezel.Cathode.Diagnostics;

[StackTraceHidden]
internal static class Assert
{
    public static void Always(
        [DoesNotReturnIf(false)] bool condition,
        [CallerArgumentExpression(nameof(condition))] string expression = "")
    {
        if (!condition)
            throw new AssertionException(expression);
    }

    [Conditional("DEBUG")]
    public static void Debug(
        [DoesNotReturnIf(false)] bool condition,
        [CallerArgumentExpression(nameof(condition))] string expression = "")
    {
        if (!condition)
            throw new AssertionException(expression);
    }
}
