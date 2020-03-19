using System;

namespace Microsoft.Extensions.Logging.Terminal
{
    public readonly struct TerminalLoggerEntry
    {
        public TerminalLoggerOptions Options { get; }

        public DateTime Timestamp { get; }

        public LogLevel LogLevel { get; }

        public string CategoryName { get; }

        public EventId EventId { get; }

        public string? Message { get; }

        public Exception? Exception { get; }

        internal TerminalLoggerEntry(TerminalLoggerOptions options, DateTime timestamp, LogLevel logLevel,
            string categoryName, EventId eventId, string? message, Exception? exception)
        {
            Options = options;
            Timestamp = timestamp;
            LogLevel = logLevel;
            CategoryName = categoryName;
            EventId = eventId;
            Message = message;
            Exception = exception;
        }
    }
}
