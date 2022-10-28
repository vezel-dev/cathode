namespace Vezel.Cathode.Extensions.Logging;

public delegate void TerminalLoggerWriter(
    TerminalLoggerOptions options, ControlBuilder builder, in TerminalLoggerMessage message);
