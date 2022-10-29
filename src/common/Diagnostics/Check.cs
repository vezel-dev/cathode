namespace Vezel.Cathode.Diagnostics;

[StackTraceHidden]
internal static class Check
{
    [InterpolatedStringHandler]
    public ref struct CheckInterpolatedStringHandler
    {
        private DefaultInterpolatedStringHandler _handler;

        public CheckInterpolatedStringHandler(
            int literalLength,
            int formattedCount,
            bool condition,
            out bool shouldAppend)
        {
            if (!condition)
            {
                _handler = new(literalLength, formattedCount);

                shouldAppend = true;
            }
            else
                shouldAppend = false;
        }

        public void AppendLiteral(string value)
        {
            _handler.AppendLiteral(value);
        }

        public void AppendFormatted<T>(T value)
        {
            _handler.AppendFormatted(value);
        }

        public void AppendFormatted<T>(T value, string? format)
        {
            _handler.AppendFormatted(value, format);
        }

        public void AppendFormatted<T>(T value, int alignment)
        {
            _handler.AppendFormatted(value, alignment);
        }

        public void AppendFormatted<T>(T value, int alignment, string? format)
        {
            _handler.AppendFormatted(value, alignment, format);
        }

        public void AppendFormatted(scoped ReadOnlySpan<char> value)
        {
            _handler.AppendFormatted(value);
        }

        public void AppendFormatted(scoped ReadOnlySpan<char> value, int alignment = 0, string? format = null)
        {
            _handler.AppendFormatted(value, alignment, format);
        }

        public void AppendFormatted(string? value)
        {
            _handler.AppendFormatted(value);
        }

        public void AppendFormatted(string? value, int alignment = 0, string? format = null)
        {
            _handler.AppendFormatted(value, alignment, format);
        }

        public void AppendFormatted(object? value, int alignment = 0, string? format = null)
        {
            _handler.AppendFormatted(value, alignment, format);
        }

        public string ToStringAndClear()
        {
            return _handler.ToStringAndClear();
        }
    }

    public static class Always
    {
        public static void Assert(
            [DoesNotReturnIf(false)] bool condition,
            [CallerArgumentExpression(nameof(condition))] string? expression = null)
        {
            if (!condition)
                throw new UnreachableException($"Hard assertion '{expression}' failed.");
        }
    }

    public static class Debug
    {
        [Conditional("DEBUG")]
        public static void Assert(
            [DoesNotReturnIf(false)] bool condition,
            [CallerArgumentExpression(nameof(condition))] string? expression = null)
        {
            if (!condition)
                throw new UnreachableException($"Debug assertion '{expression}' failed.");
        }
    }

    public static class Release
    {
        [Conditional("RELEASE")]
        public static void Assert(
            [DoesNotReturnIf(false)] bool condition,
            [CallerArgumentExpression(nameof(condition))] string? expression = null)
        {
            if (!condition)
                throw new UnreachableException($"Release assertion '{expression}' failed.");
        }
    }

    public static void Argument([DoesNotReturnIf(false)] bool condition)
    {
        if (!condition)
            throw new ArgumentException(null);
    }

    public static void Argument<T>(
        [DoesNotReturnIf(false)] bool condition,
        in T value,
        [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        _ = value;

        if (!condition)
            throw new ArgumentException(null, name);
    }

    // TODO: https://github.com/dotnet/csharplang/issues/1148
    public static void Argument<T>(
        [DoesNotReturnIf(false)] bool condition,
        scoped ReadOnlySpan<T> value,
        [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        _ = value;

        if (!condition)
            throw new ArgumentException(null, name);
    }

    public static void Null([NotNull] object? value, [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        ArgumentNullException.ThrowIfNull(value, name);
    }

    public static void NullOrEmpty(
        [NotNull] string? value, [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(value, name);
    }

    public static void Range<T>(
        [DoesNotReturnIf(false)] bool condition,
        in T value,
        [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        _ = value;

        if (!condition)
            throw new ArgumentOutOfRangeException(name);
    }

    public static void Enum<T>(T value, [CallerArgumentExpression(nameof(value))] string? name = null)
        where T : struct, Enum
    {
        if (!System.Enum.IsDefined(value))
            throw new ArgumentOutOfRangeException(name);
    }

    public static void Operation([DoesNotReturnIf(false)] bool condition)
    {
        if (!condition)
            throw new InvalidOperationException();
    }

    public static void Operation(
        [DoesNotReturnIf(false)] bool condition,
        [InterpolatedStringHandlerArgument(nameof(condition))] scoped ref CheckInterpolatedStringHandler message)
    {
        if (!condition)
            throw new InvalidOperationException(message.ToStringAndClear());
    }

    public static void All<T>(
        IEnumerable<T> collection,
        Func<T, bool> predicate,
        [CallerArgumentExpression(nameof(collection))] string? name = null)
    {
        foreach (var item in collection)
            if (!predicate(item))
                throw new ArgumentException(null, name);
    }

    public static void All<T>(
        scoped ReadOnlySpan<T> collection,
        Func<T, bool> predicate,
        [CallerArgumentExpression(nameof(collection))] string? name = null)
    {
        foreach (var item in collection)
            if (!predicate(item))
                throw new ArgumentException(null, name);
    }
}
