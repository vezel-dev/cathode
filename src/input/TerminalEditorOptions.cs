namespace System.Input;

public sealed class TerminalEditorOptions
{
    public TerminalHistory History { get; }

    public TerminalEditorOptions(TerminalHistory history)
    {
        ArgumentNullException.ThrowIfNull(history);

        History = history;
    }
}
