namespace Microsoft.Extensions.Logging
{
    readonly struct TerminalLoggerMessage
    {
        // TODO: Colors.

        public string Message { get; }

        public string? Timestamp { get; }

        public string? Level { get; }

        public bool IsError { get; }

        public TerminalLoggerMessage(string message, bool error, string? timestamp = null, string? level = null)
        {
            Message = message;
            Timestamp = timestamp;
            Level = level;
            IsError = error;
        }
    }
}
