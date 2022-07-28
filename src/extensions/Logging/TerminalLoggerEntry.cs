namespace Vezel.Cathode.Extensions.Logging;

[SuppressMessage("", "CA1815")]
internal readonly struct TerminalLoggerEntry
{
    public ReadOnlyMemory<char> Message { get; }

    public TerminalWriter Writer { get; }

    internal TerminalLoggerEntry(ReadOnlyMemory<char> message, TerminalWriter writer)
    {
        Message = message;
        Writer = writer;
    }
}
