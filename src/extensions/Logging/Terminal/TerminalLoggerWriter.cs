namespace Microsoft.Extensions.Logging.Terminal;

public delegate void TerminalLoggerWriter(
    TerminalLoggerOptions options,
    ControlBuilder builder,
    in TerminalLoggerMessage message);
