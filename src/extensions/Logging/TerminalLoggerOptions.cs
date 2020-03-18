using System;

namespace Microsoft.Extensions.Logging
{
    public sealed class TerminalLoggerOptions
    {
        TerminalLoggerFormat _format;

        LogLevel _logToStandardErrorThreshold;

        public bool DisableColors { get; set; }

        public TerminalLoggerFormat Format
        {
            get => _format;
            set
            {
                if (!Enum.IsDefined(typeof(TerminalLoggerFormat), value))
                    throw new ArgumentOutOfRangeException(nameof(value));

                _format = value;
            }
        }

        public bool IncludeScopes { get; set; }

        public LogLevel LogToStandardErrorThreshold
        {
            get => _logToStandardErrorThreshold;
            set
            {
                if (!Enum.IsDefined(typeof(TerminalLoggerFormat), value))
                    throw new ArgumentOutOfRangeException(nameof(value));

                _logToStandardErrorThreshold = value;
            }
        }

        public string? TimestampFormat { get; set; }

        public bool UseUtcTimestamp { get; set; }
    }
}
