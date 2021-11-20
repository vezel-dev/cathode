namespace Microsoft.Extensions.Logging.Terminal;

public delegate void TerminalLoggerWriter(
    TerminalLoggerOptions options,
    TerminalWriter writer,
    in TerminalLoggerEntry entry);
