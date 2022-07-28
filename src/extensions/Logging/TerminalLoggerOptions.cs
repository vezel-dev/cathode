namespace Vezel.Cathode.Extensions.Logging;

public sealed class TerminalLoggerOptions
{
    public int LogQueueSize
    {
        get => _logQueueSize;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value));

            _logQueueSize = value;
        }
    }

    public LogLevel LogToStandardErrorThreshold
    {
        get => _logToStandardErrorThreshold;
        set
        {
            _ = Enum.IsDefined(value) ? true : throw new ArgumentOutOfRangeException(nameof(value));

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
            ArgumentNullException.ThrowIfNull(value);

            _writer = value;
        }
    }

    private int _logQueueSize = 4096;

    private LogLevel _logToStandardErrorThreshold;

    private TerminalLoggerWriter _writer = TerminalLoggerWriters.Default;
}
