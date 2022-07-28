namespace Vezel.Cathode.Text.Control;

public static class ControlSequences
{
    private delegate void CreateAction<T>(ControlBuilder builder, ReadOnlySpan<T> span);

    private delegate void CreateAction<T, in TState>(ControlBuilder builder, ReadOnlySpan<T> span, TState state);

    private static readonly ThreadLocal<ControlBuilder> _builder = new(() => new());

    private static string Create<T, TState>(CreateAction<T, TState> action, ReadOnlySpan<T> span, TState state)
    {
        var cb = _builder.Value!;

        try
        {
            action(cb, span, state);

            return cb.ToString();
        }
        finally
        {
            cb.Clear();
        }
    }

    private static string Create<T>(CreateAction<T> action, ReadOnlySpan<T> span)
    {
        return Create(static (cb, span, act) => act(cb, span), span, action);
    }

    private static string Create(Action<ControlBuilder> action)
    {
        return Create(static (cb, _, act) => act(cb), ReadOnlySpan<char>.Empty, action);
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
        return Create(static (cb, title) => cb.SetTitle(title), title);
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

    public static string SetKeyboardLevel(KeyboardLevel level)
    {
        return Create(cb => cb.SetKeyboardLevel(level));
    }

    public static string SetMouseEvents(MouseEvents events)
    {
        return Create(cb => cb.SetMouseEvents(events));
    }

    public static string SetMousePointerStyle(ReadOnlySpan<char> style)
    {
        return Create(static (cb, style) => cb.SetMousePointerStyle(style), style);
    }

    public static string SetFocusEvents(bool enable)
    {
        return Create(cb => cb.SetFocusEvents(enable));
    }

    public static string SetBracketedPaste(bool enable)
    {
        return Create(cb => cb.SetBracketedPaste(enable));
    }

    public static string SetScreenBuffer(ScreenBuffer buffer)
    {
        return Create(cb => cb.SetScreenBuffer(buffer));
    }

    public static string SetInvertedColors(bool enable)
    {
        return Create(cb => cb.SetInvertedColors(enable));
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
        bool intense = false,
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
                intense,
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
        return Create(static (cb, id, uri) => cb.OpenHyperlink(uri, id), id, uri);
    }

    public static string CloseHyperlink()
    {
        return Create(cb => cb.CloseHyperlink());
    }

    public static string SaveScreenshot(ScreenshotFormat format = ScreenshotFormat.Html)
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
