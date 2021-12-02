namespace System.Text.Control;

public static class ControlSequences
{
    static readonly ThreadLocal<ControlBuilder> _builder = new(() => new());

    static string Create<T>(ReadOnlySpanAction<T, ControlBuilder> action, ReadOnlySpan<T> span)
    {
        var cb = _builder.Value!;

        try
        {
            action(span, cb);

            return cb.ToString();
        }
        finally
        {
            cb.Clear();
        }
    }

    static string Create(Action<ControlBuilder> action)
    {
        return Create((_, cb) => action(cb), ReadOnlySpan<char>.Empty);
    }

    // Keep methods in sync with the ControlStringBuilder class.

    public static string Beep()
    {
        return Create(cb => cb.Beep());
    }

    public static string Backspace()
    {
        return Create(cb => cb.Backspace());
    }

    public static string HorizontalTab()
    {
        return Create(cb => cb.HorizontalTab());
    }

    public static string LineFeed()
    {
        return Create(cb => cb.LineFeed());
    }

    public static string VerticalTab()
    {
        return Create(cb => cb.VerticalTab());
    }

    public static string FormFeed()
    {
        return Create(cb => cb.FormFeed());
    }

    public static string CarriageReturn()
    {
        return Create(cb => cb.CarriageReturn());
    }

    public static string Substitute()
    {
        return Create(cb => cb.Substitute());
    }

    public static string Cancel()
    {
        return Create(cb => cb.Cancel());
    }

    public static string FileSeparator()
    {
        return Create(cb => cb.FileSeparator());
    }

    public static string GroupSeparator()
    {
        return Create(cb => cb.GroupSeparator());
    }

    public static string RecordSeparator()
    {
        return Create(cb => cb.RecordSeparator());
    }

    public static string UnitSeparator()
    {
        return Create(cb => cb.UnitSeparator());
    }

    public static string Space()
    {
        return Create(cb => cb.Space());
    }

    public static string SetOutputBatching(bool enable)
    {
        return Create(cb => cb.SetOutputBatching(enable));
    }

    public static string SetTitle(ReadOnlySpan<char> title)
    {
        return Create((title, cb) => cb.SetTitle(title), title);
    }

    public static string PushTitle()
    {
        return Create(cb => cb.PushTitle());
    }

    public static string PopTitle()
    {
        return Create(cb => cb.PopTitle());
    }

    public static string SetProgress(ProgressState state, int value)
    {
        return Create(cb => cb.SetProgress(state, value));
    }

    public static string SetCursorKeyMode(CursorKeyMode mode)
    {
        return Create(cb => cb.SetCursorKeyMode(mode));
    }

    public static string SetKeypadMode(KeypadMode mode)
    {
        return Create(cb => cb.SetKeypadMode(mode));
    }

    public static string SetMouseEvents(MouseEvents events)
    {
        return Create(cb => cb.SetMouseEvents(events));
    }

    public static string SetMousePointerStyle(ReadOnlySpan<char> style)
    {
        return Create((style, cb) => cb.SetMousePointerStyle(style), style);
    }

    public static string SetFocusEvents(bool enable)
    {
        return Create(cb => cb.SetFocusEvents(enable));
    }

    public static string SetScreenBuffer(ScreenBuffer buffer)
    {
        return Create(cb => cb.SetScreenBuffer(buffer));
    }

    public static string SetCursorVisibility(bool visible)
    {
        return Create(cb => cb.SetCursorVisibility(visible));
    }

    public static string SetCursorStyle(CursorStyle style)
    {
        return Create(cb => cb.SetCursorStyle(style));
    }

    public static string SetScrollBarVisibility(bool visible)
    {
        return Create(cb => cb.SetScrollBarVisibility(visible));
    }

    public static string SetScrollMargin(int top, int bottom)
    {
        return Create(cb => cb.SetScrollMargin(top, bottom));
    }

    public static string InsertCharacters(int count)
    {
        return Create(cb => cb.InsertCharacters(count));
    }

    public static string DeleteCharacters(int count)
    {
        return Create(cb => cb.DeleteCharacters(count));
    }

    public static string EraseCharacters(int count)
    {
        return Create(cb => cb.EraseCharacters(count));
    }

    public static string InsertLines(int count)
    {
        return Create(cb => cb.InsertLines(count));
    }

    public static string DeleteLines(int count)
    {
        return Create(cb => cb.DeleteLines(count));
    }

    public static string ClearScreen(ClearMode mode = ClearMode.Full)
    {
        return Create(cb => cb.ClearScreen(mode));
    }

    public static string ClearLine(ClearMode mode = ClearMode.Full)
    {
        return Create(cb => cb.ClearLine(mode));
    }

    public static string MoveBufferUp(int count)
    {
        return Create(cb => cb.MoveBufferUp(count));
    }

    public static string MoveBufferDown(int count)
    {
        return Create(cb => cb.MoveBufferDown(count));
    }

    public static string MoveCursorTo(int line, int column)
    {
        return Create(cb => cb.MoveCursorTo(line, column));
    }

    public static string MoveCursorUp(int count)
    {
        return Create(cb => cb.MoveCursorUp(count));
    }

    public static string MoveCursorDown(int count)
    {
        return Create(cb => cb.MoveCursorDown(count));
    }

    public static string MoveCursorLeft(int count)
    {
        return Create(cb => cb.MoveCursorLeft(count));
    }

    public static string MoveCursorRight(int count)
    {
        return Create(cb => cb.MoveCursorRight(count));
    }

    public static string SaveCursorState()
    {
        return Create(cb => cb.SaveCursorState());
    }

    public static string RestoreCursorState()
    {
        return Create(cb => cb.RestoreCursorState());
    }

    public static string SetForegroundColor(byte r, byte g, byte b)
    {
        return Create(cb => cb.SetForegroundColor(r, g, b));
    }

    public static string SetBackgroundColor(byte r, byte g, byte b)
    {
        return Create(cb => cb.SetForegroundColor(r, g, b));
    }

    public static string SetDecorations(
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
        return Create(
            cb => cb.SetDecorations(
                bold,
                faint,
                italic,
                underline,
                blink,
                invert,
                invisible,
                strike,
                doubleUnderline,
                overline));
    }

    public static string ResetAttributes()
    {
        return Create(cb => cb.ResetAttributes());
    }

    public static string OpenHyperlink(Uri uri, ReadOnlySpan<char> id = default)
    {
        return Create((id, csb) => csb.OpenHyperlink(uri, id), id);
    }

    public static string CloseHyperlink()
    {
        return Create(cb => cb.CloseHyperlink());
    }

    public static string SaveScreenshot(ScreenshotFormat format)
    {
        return Create(cb => cb.SaveScreenshot(format));
    }

    public static string SoftReset()
    {
        return Create(cb => cb.SoftReset());
    }

    public static string FullReset()
    {
        return Create(cb => cb.FullReset());
    }
}
