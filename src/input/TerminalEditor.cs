namespace System.Input;

public sealed class TerminalEditor
{
    public TerminalEditorOptions Options { get; }

    readonly object _lock = new();

    public TerminalEditor(TerminalEditorOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        Options = options;
    }

    public string? ReadLine(string prompt, string? initial = null)
    {
        lock (_lock)
        {
            var raw = Terminal.IsRawMode;
            var events = Terminal.MouseEvents;

            Terminal.SetMouseEvents(TerminalMouseEvents.None);
            Terminal.EnableRawMode();

            try
            {
                Terminal.Out(prompt);
                Terminal.Out(initial);

                static Rune? ReadRune()
                {
                    Span<byte> bytes = stackalloc byte[Terminal.Encoding.GetMaxByteCount(1)];
                    var length = 0;

                    while (true)
                    {
                        if (Rune.DecodeFromUtf8(bytes[0..length], out var rune, out _) ==
                            OperationStatus.NeedMoreData)
                        {
                            if (Terminal.ReadRaw() is not byte b)
                                return null;

                            bytes[length++] = b;

                            continue;
                        }

                        return rune;
                    }
                }

                var list = new List<Rune>(Environment.SystemPageSize);

                if (initial != null)
                    list.AddRange(initial.EnumerateRunes());

                while (true)
                {
                    // EOF. Let the caller know.
                    if (ReadRune() is not Rune rune)
                        return null;

                    // Malformed UTF-8. Skip this rune and hope we recover.
                    if (rune == Rune.ReplacementChar)
                        continue;

                    var cp = rune.Value;

                    // Ctrl-C and Ctrl-Break/Ctrl-\
                    if (cp == 0x3)
                    {
                        Terminal.GenerateBreakSignal(TerminalBreakSignal.Interrupt);

                        return null;
                    }

                    // Enter
                    if (cp == 0xd)
                    {
                        Terminal.Out("\r\n");

                        break;
                    }

                    // Escape
                    if (cp == 0x1b)
                    {
                        // TODO: Actual editing logic.
                    }

                    // Control runes we do not recognize. Skip.
                    if (Rune.IsControl(rune))
                        continue;

                    Terminal.Out(rune);
                    list.Add(rune);
                }

                var chars = new char[list.Aggregate(0, (acc, r) => acc + r.Utf16SequenceLength)].AsSpan();
                var offset = 0;

                foreach (var rune in list)
                    offset += rune.EncodeToUtf16(chars[offset..]);

                var str = new string(chars);

                Options.History.Add(str);

                return str;
            }
            finally
            {
                Terminal.DisableRawMode();
                Terminal.SetMouseEvents(events);
            }
        }
    }
}
