namespace Microsoft.Extensions.Logging.Terminal;

public sealed class TerminalLoggerOptions
{
    readonly ref struct Decorator
    {
        readonly ControlBuilder _builder;

        readonly bool _set;

        public Decorator(ControlBuilder builder, byte r, byte g, byte b)
        {
            _builder = builder;
            _set = true;

            _ = builder.SetForegroundColor(r, g, b);
        }

        public void Dispose()
        {
            if (_set)
                _ = _builder.ResetAttributes();
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

    public bool DisableColors { get; set; }

    public bool UseUtcTimestamp { get; set; }

    public TerminalLoggerWriter Writer { get; set; } = DefaultWriter;

    static readonly ThreadLocal<ControlBuilder> _builder = new(() => new());

    LogLevel _logToStandardErrorThreshold;

    int _logQueueSize = 4096;

    public static void DefaultWriter(
        TerminalLoggerOptions options,
        TerminalWriter writer,
        in TerminalLoggerEntry entry)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(writer);

        // Is it default-initialized?
        if (entry.CategoryName == null)
            throw new ArgumentException(null, nameof(entry));

        var (lvl, r, g, b) = entry.LogLevel switch
        {
            LogLevel.Trace => ("TRC", 127, 0, 127),
            LogLevel.Debug => ("DBG", 0, 127, 255),
            LogLevel.Information => ("INF", 255, 255, 255),
            LogLevel.Warning => ("WRN", 255, 255, 0),
            LogLevel.Error => ("ERR", 255, 63, 0),
            LogLevel.Critical => ("CRT", 255, 0, 0),
            _ => throw new ArgumentException(null, nameof(entry)),
        };

        var cb = _builder.Value!;

        try
        {
            Decorator Decorate(byte r, byte g, byte b)
            {
                return !options.DisableColors ? new(cb, r, g, b) : default;
            }

            _ = cb.Print("[");

            using (_ = Decorate(127, 127, 127))
                _ = cb.Print(entry.Timestamp.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture));

            _ = cb.Print("][");

            using (_ = Decorate((byte)r, (byte)g, (byte)b))
                _ = cb.Print(lvl);

            _ = cb.Print("][");

            using (_ = Decorate(233, 233, 233))
                _ = cb.Print(entry.CategoryName);

            _ = cb.Print("][");

            using (_ = Decorate(0, 155, 155))
                _ = cb.Print(entry.EventId);

            _ = cb.Print("] ");

            if (entry.Message is string m)
                _ = cb.PrintLine(m);

            if (entry.Exception is Exception e)
                _ = cb.PrintLine(e);

            writer.Write(cb.Span);
        }
        finally
        {
            cb.Clear();
        }
    }
}
