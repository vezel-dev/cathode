namespace Microsoft.Extensions.Logging.Terminal;

[SuppressMessage("Design", "CA1815")]
readonly struct TerminalLoggerEntry
{
    public ReadOnlyMemory<char> Message { get; }

    public TerminalWriter Writer { get; }

    internal TerminalLoggerEntry(ReadOnlyMemory<char> message, TerminalWriter writer)
    {
        Message = message;
        Writer = writer;
    }
}
