namespace Vezel.Cathode.Extensions.Logging;

public sealed class TerminalLoggerOptions
{
    public int LogQueueSize
    {
        get => _logQueueSize;
        set
        {
            Check.Range(value >= 0, value);

            _logQueueSize = value;
        }
    }

    public LogLevel LogToStandardErrorThreshold
    {
        get => _logToStandardErrorThreshold;
        set
        {
            Check.Enum(value);

            _logToStandardErrorThreshold = value;
        }
    }

    public bool UseColors { get; set; } = true;

    public bool SingleLine { get; set; }

    public bool UseUtcTimestamp { get; set; }

    public TerminalLoggerWriter Writer
    {
        get => _writer;
        set
        {
            Check.Null(value);

            _writer = value;
        }
    }

    private int _logQueueSize = 4096;

    private LogLevel _logToStandardErrorThreshold = LogLevel.None;

    private TerminalLoggerWriter _writer = TerminalLoggerWriters.Default;
}
