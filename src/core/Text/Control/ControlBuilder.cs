using static System.Text.Control.ControlConstants;

namespace System.Text.Control;

public sealed class ControlBuilder
{
    // TODO: We are missing many escape sequences.

    public ReadOnlySpan<char> Span => _writer.WrittenSpan;

    readonly ArrayBufferWriter<char> _writer;

    public ControlBuilder(int capacity = 1024)
    {
        _ = capacity > 0 ? true : throw new ArgumentOutOfRangeException(nameof(capacity));

        _writer = new(capacity);
    }

    public void Clear()
    {
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

    public ControlBuilder Print(string? value)
    {
        return Print(value.AsSpan());
    }

    public ControlBuilder Print<T>(T value)
    {
        return Print(value?.ToString());
    }

    public ControlBuilder Print(string format, params object?[] args)
    {
        return Print(string.Format(CultureInfo.CurrentCulture, format, args));
    }

    public ControlBuilder PrintLine()
    {
        return PrintLine(null);
    }

    public ControlBuilder PrintLine(string? value)
    {
        return Print(value + Environment.NewLine);
    }

    public ControlBuilder PrintLine<T>(T value)
    {
        return PrintLine(value?.ToString());
    }

    public ControlBuilder PrintLine(string format, params object?[] args)
    {
        return PrintLine(string.Format(CultureInfo.CurrentCulture, format, args));
    }

    // Keep methods in sync with the ControlSequences class.

    public ControlBuilder Beep()
    {
        return Print(BEL);
    }

    public ControlBuilder Backspace()
    {
        return Print(BS);
    }

    public ControlBuilder HorizontalTab()
    {
        return Print(HT);
    }

    public ControlBuilder LineFeed()
    {
        return Print(LF);
    }

    public ControlBuilder VerticalTab()
    {
        return Print(VT);
    }

    public ControlBuilder FormFeed()
    {
        return Print(FF);
    }

    public ControlBuilder CarriageReturn()
    {
        return Print(CR);
    }

    public ControlBuilder FileSeparator()
    {
        return Print(FS);
    }

    public ControlBuilder GroupSeparator()
    {
        return Print(GS);
    }

    public ControlBuilder RecordSeparator()
    {
        return Print(RS);
    }

    public ControlBuilder UnitSeparator()
    {
        return Print(US);
    }

    public ControlBuilder Space()
    {
        return Print(SP);
    }

    public ControlBuilder SetOutputBatching(bool enable)
    {
        return Print(CSI).Print("?2026").Print(enable ? "h" : "l");
    }

    public ControlBuilder SetTitle(string title)
    {
        ArgumentNullException.ThrowIfNull(title);

        return Print(OSC).Print("2;").Print(title).Print(ST);
    }

    public ControlBuilder SetProgress(ProgressState state, int value)
    {
        _ = Math.Clamp(value, 0, 100) == value ? true : throw new ArgumentOutOfRangeException(nameof(value));

        Span<char> stateSpan = stackalloc char[32];
        Span<char> valueSpan = stackalloc char[32];

        _ = ((int)state).TryFormat(stateSpan, out var stateLen);
        _ = value.TryFormat(valueSpan, out var valueLen);

        return Print(OSC).Print("9;4;").Print(stateSpan[..stateLen]).Print(";").Print(valueSpan[..valueLen]).Print(ST);
    }

    public ControlBuilder SetCursorKeyMode(CursorKeyMode mode)
    {
        _ = Enum.IsDefined(mode) ? true : throw new ArgumentOutOfRangeException(nameof(mode));

        var ch = (char)mode;

        return Print(CSI).Print("?1").Print(MemoryMarshal.CreateSpan(ref ch, 1));
    }

    public ControlBuilder SetKeypadMode(KeypadMode mode)
    {
        _ = Enum.IsDefined(mode) ? true : throw new ArgumentOutOfRangeException(nameof(mode));

        var ch = (char)mode;

        return Print(ESC).Print(MemoryMarshal.CreateSpan(ref ch, 1));
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

    public ControlBuilder SetScreenBuffer(ScreenBuffer buffer)
    {
        _ = Enum.IsDefined(buffer) ? true : throw new ArgumentOutOfRangeException(nameof(buffer));

        var ch = (char)buffer;

        return Print(CSI).Print("?1049").Print(MemoryMarshal.CreateSpan(ref ch, 1));
    }

    public ControlBuilder SetCursorVisibility(bool visible)
    {
        return Print(CSI).Print("?25").Print(visible ? "h" : "l");
    }

    public ControlBuilder SetCursorStyle(CursorStyle style)
    {
        _ = Enum.IsDefined(style) ? true : throw new ArgumentOutOfRangeException(nameof(style));

        Span<char> styleSpan = stackalloc char[32];

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

        Span<char> topSpan = stackalloc char[32];
        Span<char> bottomSpan = stackalloc char[32];

        _ = (top + 1).TryFormat(topSpan, out var topLen);
        _ = (bottom + 1).TryFormat(bottomSpan, out var bottomLen);

        return Print(CSI).Print(topSpan[..topLen]).Print(";").Print(bottomSpan[..bottomLen]).Print("r");
    }

    ControlBuilder ModifyText(string type, int count)
    {
        _ = count >= 0 ? true : throw new ArgumentOutOfRangeException(nameof(count));

        if (count == 0)
            return this;

        Span<char> countSpan = stackalloc char[32];

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

        Span<char> modeSpan = stackalloc char[32];

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

        Span<char> countSpan = stackalloc char[32];

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

        Span<char> lineSpan = stackalloc char[32];
        Span<char> columnSpan = stackalloc char[32];

        _ = (line + 1).TryFormat(lineSpan, out var lineLen);
        _ = (column + 1).TryFormat(columnSpan, out var columnLen);

        return Print(CSI).Print(columnSpan[..columnLen]).Print(";").Print(lineSpan[..lineLen]).Print("H");
    }

    ControlBuilder MoveCursor(string type, int count)
    {
        _ = count >= 0 ? true : throw new ArgumentOutOfRangeException(nameof(count));

        if (count == 0)
            return this;

        Span<char> countSpan = stackalloc char[32];

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
        Span<char> rSpan = stackalloc char[32];
        Span<char> gSpan = stackalloc char[32];
        Span<char> bSpan = stackalloc char[32];

        _ = r.TryFormat(rSpan, out var rLen);
        _ = g.TryFormat(gSpan, out var gLen);
        _ = b.TryFormat(bSpan, out var bLen);

        return Print(CSI).Print("38;2;").Print(rSpan[..rLen]).Print(";")
            .Print(gSpan[..gLen]).Print(";").Print(bSpan[..bLen]).Print("m");
    }

    public ControlBuilder SetBackgroundColor(byte r, byte g, byte b)
    {
        Span<char> rSpan = stackalloc char[32];
        Span<char> gSpan = stackalloc char[32];
        Span<char> bSpan = stackalloc char[32];

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

    public ControlBuilder SaveScreenshot(ScreenshotFormat format)
    {
        _ = Enum.IsDefined(format) ? true : throw new ArgumentOutOfRangeException(nameof(format));

        Span<char> formatSpan = stackalloc char[32];

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
