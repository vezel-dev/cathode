namespace System.Input;

public sealed class TerminalEditorOptions
{
    public TerminalHistory History { get; }

    public TerminalEditorOptions(TerminalHistory history)
    {
        History = history ?? throw new ArgumentNullException(nameof(history));
    }
}
