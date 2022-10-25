namespace Vezel.Cathode.Extensions.Logging;

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
