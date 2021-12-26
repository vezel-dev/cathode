using static System.Text.Control.ControlConstants;

namespace System.Text.Control;

public sealed class ControlBuilder
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [InterpolatedStringHandler]
    public readonly ref struct PrintInterpolatedStringHandler
    {
        const int StackBufferSize = 256;

        readonly ControlBuilder _builder;

        readonly IFormatProvider? _provider;

        readonly ICustomFormatter? _formatter;

        public PrintInterpolatedStringHandler(
            int literalLength,
            int formattedCount,
            ControlBuilder builder,
            IFormatProvider? provider = null)
        {
            _builder = builder;
            _provider = provider;
            _formatter = provider is not CultureInfo ?
                (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter)) : null;
        }

        void AppendSpan(ReadOnlySpan<char> span)
        {
            _ = _builder.Print(span);
        }

        public void AppendLiteral(string value)
        {
            AppendSpan(value);
        }

        [SuppressMessage("Style", "IDE0038")]
        public void AppendFormatted<T>(T value, string? format = null)
        {
            if (_formatter != null)
            {
                AppendSpan(_formatter.Format(format, value, _provider));

                return;
            }

            // Do not use pattern matching here as it results in boxing.
            if (value is IFormattable)
            {
                if (value is ISpanFormattable)
                {
                    char[]? rented = null;
                    Span<char> span = stackalloc char[StackBufferSize];

                    try
                    {
                        int written;

                        // Try to format the value on the stack; fall back to the heap.
                        while (!((ISpanFormattable)value).TryFormat(span, out written, format, _provider))
                        {
                            if (rented != null)
                                ArrayPool<char>.Shared.Return(rented);

                            var len = span.Length * 2;

                            rented = ArrayPool<char>.Shared.Rent(len);
                            span = rented.AsSpan(0, len);
                        }

                        AppendSpan(span[..written]);
                    }
                    finally
                    {
                        if (rented != null)
                            ArrayPool<char>.Shared.Return(rented);
                    }
                }
                else
                    AppendSpan(((IFormattable)value).ToString(format, _provider));
            }
            else
                AppendSpan(value?.ToString());
        }

        public void AppendFormatted(object? value, string? format = null)
        {
            // This overload is used when a target-typed expression cannot use the generic overload.
            AppendFormatted<object?>(value, format);
        }

        public void AppendFormatted(string? value)
        {
            // This overload exists to disambiguate string since it can implicitly convert to both object and
            // ReadOnlySpan<char>.
            AppendFormatted<string?>(value);
        }

        public unsafe void AppendFormatted(void* value, string? format = null)
        {
            // This overload makes pointer values work in interpolation holes; they cannot be passed as generic type
            // arguments currently.
            AppendFormatted((nuint)value, format);
        }

        public void AppendFormatted(ReadOnlySpan<char> value)
        {
            AppendSpan(value);
        }
    }

    const int StackBufferSize = 32;

    public ReadOnlySpan<char> Span => _writer.WrittenSpan;

    readonly int _capacity;

    ArrayBufferWriter<char> _writer;

    public ControlBuilder(int capacity = 1024)
    {
        _ = capacity > 0 ? true : throw new ArgumentOutOfRangeException(nameof(capacity));

        _capacity = capacity;
        _writer = new(capacity);
    }

    public void Clear(int reallocateThreshold = 4096)
    {
        _ = reallocateThreshold >= 0 ? true : throw new ArgumentOutOfRangeException(nameof(reallocateThreshold));

        if (reallocateThreshold != 0 && _writer.Capacity > reallocateThreshold)
            _writer = new(_capacity);
        else
            _writer.Clear();
    }

    public override string ToString()
    {
        return Span.ToString();
    }

    public ControlBuilder Print(ReadOnlySpan<char> value)
    {
        _writer.Write(value);

        return this;
    }

    public ControlBuilder Print<T>(T value)
    {
        return Print((value?.ToString()).AsSpan());
    }

    public ControlBuilder Print([InterpolatedStringHandlerArgument("")] ref PrintInterpolatedStringHandler handler)
    {
        return this;
    }

    public ControlBuilder Print(
        IFormatProvider? provider,
        [InterpolatedStringHandlerArgument("", "provider")] ref PrintInterpolatedStringHandler handler)
    {
        return this;
    }

    public ControlBuilder PrintLine()
    {
        return Print(Environment.NewLine);
    }

    public ControlBuilder PrintLine<T>(T value)
    {
        return Print(value).PrintLine();
    }

    public ControlBuilder PrintLine([InterpolatedStringHandlerArgument("")] ref PrintInterpolatedStringHandler handler)
    {
        return PrintLine();
    }

    public ControlBuilder PrintLine(
        IFormatProvider? provider,
        [InterpolatedStringHandlerArgument("", "provider")] ref PrintInterpolatedStringHandler handler)
    {
        return PrintLine();
    }

    // Keep methods in sync with the ControlSequences class.

    public ControlBuilder Null()
    {
        return Print(stackalloc[] { NUL });
    }

    public ControlBuilder Beep()
    {
        return Print(stackalloc[] { BEL });
    }

    public ControlBuilder Backspace()
    {
        return Print(stackalloc[] { BS });
    }

    public ControlBuilder HorizontalTab()
    {
        return Print(stackalloc[] { HT });
    }

    public ControlBuilder LineFeed()
    {
        return Print(stackalloc[] { LF });
    }

    public ControlBuilder VerticalTab()
    {
        return Print(stackalloc[] { VT });
    }

    public ControlBuilder FormFeed()
    {
        return Print(stackalloc[] { FF });
    }

    public ControlBuilder CarriageReturn()
    {
        return Print(stackalloc[] { CR });
    }

    public ControlBuilder Substitute()
    {
        return Print(stackalloc[] { SUB });
    }

    public ControlBuilder Cancel()
    {
        return Print(stackalloc[] { CAN });
    }

    public ControlBuilder FileSeparator()
    {
        return Print(stackalloc[] { FS });
    }

    public ControlBuilder GroupSeparator()
    {
        return Print(stackalloc[] { GS });
    }

    public ControlBuilder RecordSeparator()
    {
        return Print(stackalloc[] { RS });
    }

    public ControlBuilder UnitSeparator()
    {
        return Print(stackalloc[] { US });
    }

    public ControlBuilder Space()
    {
        return Print(stackalloc[] { SP });
    }

    public ControlBuilder SetOutputBatching(bool enable)
    {
        return Print(CSI).Print("?2026").Print(enable ? "h" : "l");
    }

    public ControlBuilder SetTitle(ReadOnlySpan<char> title)
    {
        return Print(OSC).Print("2;").Print(title).Print(ST);
    }

    public ControlBuilder PushTitle()
    {
        return Print(CSI).Print("22;2t");
    }

    public ControlBuilder PopTitle()
    {
        return Print(CSI).Print("23;2t");
    }

    public ControlBuilder SetProgress(ProgressState state, int value)
    {
        _ = Math.Clamp(value, 0, 100) == value ? true : throw new ArgumentOutOfRangeException(nameof(value));

        Span<char> stateSpan = stackalloc char[StackBufferSize];
        Span<char> valueSpan = stackalloc char[StackBufferSize];

        _ = ((int)state).TryFormat(stateSpan, out var stateLen);
        _ = value.TryFormat(valueSpan, out var valueLen);

        return Print(OSC).Print("9;4;").Print(stateSpan[..stateLen]).Print(";").Print(valueSpan[..valueLen]).Print(ST);
    }

    public ControlBuilder SetCursorKeyMode(CursorKeyMode mode)
    {
        _ = Enum.IsDefined(mode) ? true : throw new ArgumentOutOfRangeException(nameof(mode));

        var ch = (char)mode;

        return Print(CSI).Print("?1").Print(MemoryMarshal.CreateReadOnlySpan(ref ch, 1));
    }

    public ControlBuilder SetKeypadMode(KeypadMode mode)
    {
        _ = Enum.IsDefined(mode) ? true : throw new ArgumentOutOfRangeException(nameof(mode));

        var ch = (char)mode;

        return Print(ESC).Print(MemoryMarshal.CreateReadOnlySpan(ref ch, 1));
    }

    public ControlBuilder SetKeyboardLevel(KeyboardLevel level)
    {
        var (cursor, function, other) = level switch
        {
            KeyboardLevel.Basic => ("1n", "2n", "4n"),
            KeyboardLevel.Normal => ("1;2m", "2;2m", "4;0m"),
            KeyboardLevel.Extended => ("1;2m", "2;2m", "4;2m"),
            _ => throw new ArgumentOutOfRangeException(nameof(level)),
        };

        return Print(CSI).Print(cursor).Print(CSI).Print(function).Print(CSI).Print(other);
    }

    public ControlBuilder SetMouseEvents(MouseEvents events)
    {
        return Print(CSI).Print("?1003").Print(events.HasFlag(MouseEvents.Movement) ? "h" : "l")
            .Print(CSI).Print("?1006").Print(events.HasFlag(MouseEvents.Buttons) ? "h" : "l");
    }

    public ControlBuilder SetMousePointerStyle(ReadOnlySpan<char> style)
    {
        return Print(OSC).Print("22;").Print(style).Print(ST);
    }

    public ControlBuilder SetFocusEvents(bool enable)
    {
        return Print(CSI).Print("?1004").Print(enable ? "h" : "l");
    }

    public ControlBuilder SetBracketedPaste(bool enable)
    {
        return Print(CSI).Print("?2004").Print(enable ? "h" : "l");
    }

    public ControlBuilder SetScreenBuffer(ScreenBuffer buffer)
    {
        _ = Enum.IsDefined(buffer) ? true : throw new ArgumentOutOfRangeException(nameof(buffer));

        var ch = (char)buffer;

        return Print(CSI).Print("?1049").Print(MemoryMarshal.CreateReadOnlySpan(ref ch, 1));
    }

    public ControlBuilder SetInvertedColors(bool enable)
    {
        return Print(CSI).Print("?5").Print(enable ? "h" : "l");
    }

    public ControlBuilder SetCursorVisibility(bool visible)
    {
        return Print(CSI).Print("?25").Print(visible ? "h" : "l");
    }

    public ControlBuilder SetCursorStyle(CursorStyle style)
    {
        _ = Enum.IsDefined(style) ? true : throw new ArgumentOutOfRangeException(nameof(style));

        Span<char> styleSpan = stackalloc char[StackBufferSize];

        _ = ((int)style).TryFormat(styleSpan, out var styleLen);

        return Print(CSI).Print(styleSpan[..styleLen]).Space().Print("q");
    }

    public ControlBuilder SetScrollBarVisibility(bool visible)
    {
        return Print(CSI).Print("?30").Print(visible ? "h" : "l");
    }

    public ControlBuilder SetScrollMargin(int top, int bottom)
    {
        _ = top >= 0 ? true : throw new ArgumentOutOfRangeException(nameof(top));
        _ = bottom >= 0 ? true : throw new ArgumentOutOfRangeException(nameof(bottom));
        _ = bottom >= top ? true : throw new ArgumentException(null, nameof(bottom));

        Span<char> topSpan = stackalloc char[StackBufferSize];
        Span<char> bottomSpan = stackalloc char[StackBufferSize];

        _ = (top + 1).TryFormat(topSpan, out var topLen);
        _ = (bottom + 1).TryFormat(bottomSpan, out var bottomLen);

        return Print(CSI).Print(topSpan[..topLen]).Print(";").Print(bottomSpan[..bottomLen]).Print("r");
    }

    ControlBuilder ModifyText(string type, int count)
    {
        _ = count >= 0 ? true : throw new ArgumentOutOfRangeException(nameof(count));

        if (count == 0)
            return this;

        Span<char> countSpan = stackalloc char[StackBufferSize];

        _ = count.TryFormat(countSpan, out var countLen);

        return Print(CSI).Print(countSpan[..countLen]).Print(type);
    }

    public ControlBuilder InsertCharacters(int count)
    {
        return ModifyText("@", count);
    }

    public ControlBuilder DeleteCharacters(int count)
    {
        return ModifyText("P", count);
    }

    public ControlBuilder EraseCharacters(int count)
    {
        return ModifyText("X", count);
    }

    public ControlBuilder InsertLines(int count)
    {
        return ModifyText("L", count);
    }

    public ControlBuilder DeleteLines(int count)
    {
        return ModifyText("M", count);
    }

    ControlBuilder Clear(string type, ClearMode mode)
    {
        _ = Enum.IsDefined(mode) ? true : throw new ArgumentOutOfRangeException(nameof(mode));

        Span<char> modeSpan = stackalloc char[StackBufferSize];

        _ = ((int)mode).TryFormat(modeSpan, out var modeLen);

        return Print(CSI).Print(modeSpan[..modeLen]).Print(type);
    }

    public ControlBuilder ClearScreen(ClearMode mode = ClearMode.Full)
    {
        return Clear("J", mode);
    }

    public ControlBuilder ClearLine(ClearMode mode = ClearMode.Full)
    {
        return Clear("K", mode);
    }

    ControlBuilder MoveBuffer(string type, int count)
    {
        _ = count >= 0 ? true : throw new ArgumentOutOfRangeException(nameof(count));

        if (count == 0)
            return this;

        Span<char> countSpan = stackalloc char[StackBufferSize];

        _ = count.TryFormat(countSpan, out var countLen);

        return Print(CSI).Print(countSpan[..countLen]).Print(type);
    }

    public ControlBuilder MoveBufferUp(int count)
    {
        return MoveBuffer("S", count);
    }

    public ControlBuilder MoveBufferDown(int count)
    {
        return MoveBuffer("T", count);
    }

    public ControlBuilder MoveCursorTo(int line, int column)
    {
        _ = line >= 0 ? true : throw new ArgumentOutOfRangeException(nameof(line));
        _ = column >= 0 ? true : throw new ArgumentOutOfRangeException(nameof(column));

        Span<char> lineSpan = stackalloc char[StackBufferSize];
        Span<char> columnSpan = stackalloc char[StackBufferSize];

        _ = (line + 1).TryFormat(lineSpan, out var lineLen);
        _ = (column + 1).TryFormat(columnSpan, out var columnLen);

        return Print(CSI).Print(columnSpan[..columnLen]).Print(";").Print(lineSpan[..lineLen]).Print("H");
    }

    ControlBuilder MoveCursor(string type, int count)
    {
        _ = count >= 0 ? true : throw new ArgumentOutOfRangeException(nameof(count));

        if (count == 0)
            return this;

        Span<char> countSpan = stackalloc char[StackBufferSize];

        _ = count.TryFormat(countSpan, out var countLen);

        return Print(CSI).Print(countSpan[..countLen]).Print(type);
    }

    public ControlBuilder MoveCursorUp(int count)
    {
        return MoveCursor("A", count);
    }

    public ControlBuilder MoveCursorDown(int count)
    {
        return MoveCursor("B", count);
    }

    public ControlBuilder MoveCursorLeft(int count)
    {
        return MoveCursor("D", count);
    }

    public ControlBuilder MoveCursorRight(int count)
    {
        return MoveCursor("C", count);
    }

    public ControlBuilder SaveCursorState()
    {
        return Print(ESC).Print("7");
    }

    public ControlBuilder RestoreCursorState()
    {
        return Print(ESC).Print("8");
    }

    public ControlBuilder SetForegroundColor(byte r, byte g, byte b)
    {
        Span<char> rSpan = stackalloc char[StackBufferSize];
        Span<char> gSpan = stackalloc char[StackBufferSize];
        Span<char> bSpan = stackalloc char[StackBufferSize];

        _ = r.TryFormat(rSpan, out var rLen);
        _ = g.TryFormat(gSpan, out var gLen);
        _ = b.TryFormat(bSpan, out var bLen);

        return Print(CSI).Print("38;2;").Print(rSpan[..rLen]).Print(";")
            .Print(gSpan[..gLen]).Print(";").Print(bSpan[..bLen]).Print("m");
    }

    public ControlBuilder SetBackgroundColor(byte r, byte g, byte b)
    {
        Span<char> rSpan = stackalloc char[StackBufferSize];
        Span<char> gSpan = stackalloc char[StackBufferSize];
        Span<char> bSpan = stackalloc char[StackBufferSize];

        _ = r.TryFormat(rSpan, out var rLen);
        _ = g.TryFormat(gSpan, out var gLen);
        _ = b.TryFormat(bSpan, out var bLen);

        return Print(CSI).Print("48;2;").Print(rSpan[..rLen]).Print(";")
            .Print(gSpan[..gLen]).Print(";").Print(bSpan[..bLen]).Print("m");
    }

    public ControlBuilder SetDecorations(
        bool bold = false,
        bool faint = false,
        bool italic = false,
        bool underline = false,
        bool blink = false,
        bool invert = false,
        bool invisible = false,
        bool strike = false,
        bool doubleUnderline = false,
        bool overline = false)
    {
        _ = Print(CSI);

        var i = 0;

        void HandleMode(bool value, ReadOnlySpan<char> code)
        {
            if (!value)
                return;

            if (i != 0)
                _ = Print(";");

            i++;

            _ = Print(code);
        }

        // TODO: Support more exotic decorations?
        HandleMode(bold, "1");
        HandleMode(faint, "2");
        HandleMode(italic, "3");
        HandleMode(underline, "4");
        HandleMode(blink, "5");
        HandleMode(invert, "7");
        HandleMode(invisible, "8");
        HandleMode(strike, "9");
        HandleMode(doubleUnderline, "21");
        HandleMode(overline, "53");

        return Print("m");
    }

    public ControlBuilder ResetAttributes()
    {
        return Print(CSI).Print("0m");
    }

    public ControlBuilder OpenHyperlink(Uri uri, ReadOnlySpan<char> id = default)
    {
        ArgumentNullException.ThrowIfNull(uri);

        _ = Print(OSC).Print("8;");

        if (!id.IsEmpty)
            _ = Print("id=").Print(id);

        // TODO: Avoid string allocation for the URI value.
        return Print(";").Print(uri).Print(ST);
    }

    public ControlBuilder CloseHyperlink()
    {
        return Print(OSC).Print("8;;").Print(ST);
    }

    public ControlBuilder SaveScreenshot(ScreenshotFormat format = ScreenshotFormat.Html)
    {
        _ = Enum.IsDefined(format) ? true : throw new ArgumentOutOfRangeException(nameof(format));

        Span<char> formatSpan = stackalloc char[StackBufferSize];

        _ = ((int)format).TryFormat(formatSpan, out var formatLen);

        return Print(CSI).Print(formatSpan[..formatLen]).Print("i");
    }

    public ControlBuilder SoftReset()
    {
        return Print(CSI).Print("!p");
    }

    public ControlBuilder FullReset()
    {
        return Print(ESC).Print("c");
    }
}
