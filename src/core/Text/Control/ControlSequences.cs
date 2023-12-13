namespace Vezel.Cathode.Text.Control;

public static class ControlSequences
{
    private delegate void CreateAction<T>(ControlBuilder builder, ReadOnlySpan<T> span);

    private delegate void CreateAction<T, in TState>(ControlBuilder builder, ReadOnlySpan<T> span, TState state);

    [ThreadStatic]
    private static ControlBuilder? _builder;

    private static string Create<T, TState>(CreateAction<T, TState> action, scoped ReadOnlySpan<T> span, TState state)
    {
        var cb = _builder ??= new();

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

    private static string Create<T>(CreateAction<T> action, scoped ReadOnlySpan<T> span)
    {
        return Create(static (cb, span, action) => action(cb, span), span, action);
    }

    private static string Create<TState>(Action<ControlBuilder, TState> action, TState state)
    {
        return Create(
            static (cb, _, args) => args.Action(cb, args.State),
            ReadOnlySpan<char>.Empty,
            (Action: action, State: state));
    }

    private static string Create(Action<ControlBuilder> action)
    {
        return Create(static (cb, _, action) => action(cb), ReadOnlySpan<char>.Empty, action);
    }

    // Keep methods in sync with the ControlBuilder class.

    public static string Beep()
    {
        return Create(static cb => cb.Beep());
    }

    public static string Backspace()
    {
        return Create(static cb => cb.Backspace());
    }

    public static string HorizontalTab()
    {
        return Create(static cb => cb.HorizontalTab());
    }

    public static string LineFeed()
    {
        return Create(static cb => cb.LineFeed());
    }

    public static string VerticalTab()
    {
        return Create(static cb => cb.VerticalTab());
    }

    public static string FormFeed()
    {
        return Create(static cb => cb.FormFeed());
    }

    public static string CarriageReturn()
    {
        return Create(static cb => cb.CarriageReturn());
    }

    public static string Substitute()
    {
        return Create(static cb => cb.Substitute());
    }

    public static string Cancel()
    {
        return Create(static cb => cb.Cancel());
    }

    public static string FileSeparator()
    {
        return Create(static cb => cb.FileSeparator());
    }

    public static string GroupSeparator()
    {
        return Create(static cb => cb.GroupSeparator());
    }

    public static string RecordSeparator()
    {
        return Create(static cb => cb.RecordSeparator());
    }

    public static string UnitSeparator()
    {
        return Create(static cb => cb.UnitSeparator());
    }

    public static string Space()
    {
        return Create(static cb => cb.Space());
    }

    public static string SetOutputBatching(bool enable)
    {
        return Create(static (cb, enable) => cb.SetOutputBatching(enable), enable);
    }

    public static string SetTitle(scoped ReadOnlySpan<char> title)
    {
        return Create(static (cb, title) => cb.SetTitle(title), title);
    }

    public static string PushTitle()
    {
        return Create(static cb => cb.PushTitle());
    }

    public static string PopTitle()
    {
        return Create(static cb => cb.PopTitle());
    }

    public static string SetProgress(ProgressState state, int value)
    {
        return Create(static (cb, args) => cb.SetProgress(args.state, args.value), (state, value));
    }

    public static string SetCursorKeyMode(CursorKeyMode mode)
    {
        return Create(static (cb, mode) => cb.SetCursorKeyMode(mode), mode);
    }

    public static string SetKeypadMode(KeypadMode mode)
    {
        return Create(static (cb, mode) => cb.SetKeypadMode(mode), mode);
    }

    public static string SetKeyboardLevel(KeyboardLevel level)
    {
        return Create(static (cb, level) => cb.SetKeyboardLevel(level), level);
    }

    public static string SetAutoRepeatMode(bool enable)
    {
        return Create(static (cb, enable) => cb.SetAutoRepeatMode(enable), enable);
    }

    public static string SetMouseEvents(MouseEvents events)
    {
        return Create(static (cb, events) => cb.SetMouseEvents(events), events);
    }

    public static string SetMousePointerStyle(scoped ReadOnlySpan<char> style)
    {
        return Create(static (cb, style) => cb.SetMousePointerStyle(style), style);
    }

    public static string SetFocusEvents(bool enable)
    {
        return Create(static (cb, enable) => cb.SetFocusEvents(enable), enable);
    }

    public static string SetBracketedPaste(bool enable)
    {
        return Create(static (cb, enable) => cb.SetBracketedPaste(enable), enable);
    }

    public static string SetScreenBuffer(ScreenBuffer buffer)
    {
        return Create(static (cb, buffer) => cb.SetScreenBuffer(buffer), buffer);
    }

    public static string SetInvertedColors(bool enable)
    {
        return Create(static (cb, enable) => cb.SetInvertedColors(enable), enable);
    }

    public static string SetCursorVisibility(bool visible)
    {
        return Create(static (cb, visible) => cb.SetCursorVisibility(visible), visible);
    }

    public static string SetCursorStyle(CursorStyle style)
    {
        return Create(static (cb, style) => cb.SetCursorStyle(style), style);
    }

    public static string SetScrollBarVisibility(bool visible)
    {
        return Create(static (cb, visible) => cb.SetScrollBarVisibility(visible), visible);
    }

    public static string SetScrollMargin(int top, int bottom)
    {
        return Create(static (cb, args) => cb.SetScrollMargin(args.top, args.bottom), (top, bottom));
    }

    public static string InsertCharacters(int count)
    {
        return Create(static (cb, count) => cb.InsertCharacters(count), count);
    }

    public static string DeleteCharacters(int count)
    {
        return Create(static (cb, count) => cb.DeleteCharacters(count), count);
    }

    public static string EraseCharacters(int count)
    {
        return Create(static (cb, count) => cb.EraseCharacters(count), count);
    }

    public static string InsertLines(int count)
    {
        return Create(static (cb, count) => cb.InsertLines(count), count);
    }

    public static string DeleteLines(int count)
    {
        return Create(static (cb, count) => cb.DeleteLines(count), count);
    }

    public static string ClearScreen(ClearMode mode = ClearMode.Full)
    {
        return Create(static (cb, mode) => cb.ClearScreen(mode), mode);
    }

    public static string ClearLine(ClearMode mode = ClearMode.Full)
    {
        return Create(static (cb, mode) => cb.ClearLine(mode), mode);
    }

    public static string SetProtection(bool protect)
    {
        return Create(static (cb, protect) => cb.SetProtection(protect), protect);
    }

    public static string ProtectedClearScreen(ClearMode mode = ClearMode.Full)
    {
        return Create(static (cb, mode) => cb.ProtectedClearScreen(mode), mode);
    }

    public static string ProtectedClearLine(ClearMode mode = ClearMode.Full)
    {
        return Create(static (cb, mode) => cb.ProtectedClearLine(mode), mode);
    }

    public static string MoveBufferUp(int count)
    {
        return Create(static (cb, count) => cb.MoveBufferUp(count), count);
    }

    public static string MoveBufferDown(int count)
    {
        return Create(static (cb, count) => cb.MoveBufferDown(count), count);
    }

    public static string MoveCursorTo(int line, int column)
    {
        return Create(static (cb, args) => cb.MoveCursorTo(args.line, args.column), (line, column));
    }

    public static string MoveCursorUp(int count)
    {
        return Create(static (cb, count) => cb.MoveCursorUp(count), count);
    }

    public static string MoveCursorDown(int count)
    {
        return Create(static (cb, count) => cb.MoveCursorDown(count), count);
    }

    public static string MoveCursorLeft(int count)
    {
        return Create(static (cb, count) => cb.MoveCursorLeft(count), count);
    }

    public static string MoveCursorRight(int count)
    {
        return Create(static (cb, count) => cb.MoveCursorRight(count), count);
    }

    public static string SaveCursorState()
    {
        return Create(static cb => cb.SaveCursorState());
    }

    public static string RestoreCursorState()
    {
        return Create(static cb => cb.RestoreCursorState());
    }

    public static string SetForegroundColor(Color color)
    {
        return Create(static (cb, color) => cb.SetForegroundColor(color), color);
    }

    public static string SetBackgroundColor(Color color)
    {
        return Create(static (cb, color) => cb.SetForegroundColor(color), color);
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
            static (cb, args) =>
                cb.SetDecorations(
                    args.intense,
                    args.faint,
                    args.italic,
                    args.underline,
                    args.blink,
                    args.invert,
                    args.invisible,
                    args.strike,
                    args.doubleUnderline,
                    args.overline),
            (intense,
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
        return Create(static cb => cb.ResetAttributes());
    }

    public static string OpenHyperlink(Uri uri, scoped ReadOnlySpan<char> id = default)
    {
        return Create(static (cb, id, uri) => cb.OpenHyperlink(uri, id), id, uri);
    }

    public static string CloseHyperlink()
    {
        return Create(static cb => cb.CloseHyperlink());
    }

    public static string SaveScreenshot(ScreenshotFormat format = ScreenshotFormat.Html)
    {
        return Create(static (cb, format) => cb.SaveScreenshot(format), format);
    }

    public static string PlayNotes(int volume, int duration, scoped ReadOnlySpan<int> notes)
    {
        return Create(
            static (cb, notes, args) => cb.PlayNotes(args.volume, args.duration, notes), notes, (volume, duration));
    }

    public static string SoftReset()
    {
        return Create(static cb => cb.SoftReset());
    }

    public static string FullReset()
    {
        return Create(static cb => cb.FullReset());
    }
}
