namespace Vezel.Cathode.Extensions.Logging;

public static class TerminalLoggerWriters
{
    private readonly ref struct Decorator
    {
        private readonly ControlBuilder _builder;

        private readonly bool _set;

        public Decorator(ControlBuilder builder, Color color)
        {
            _builder = builder;
            _set = true;

            _ = builder.SetForegroundColor(color);
        }

        public void Dispose()
        {
            if (_set)
                _ = _builder.ResetAttributes();
        }
    }

    public static void Default(TerminalLoggerOptions options, ControlBuilder builder, in TerminalLoggerMessage message)
    {
        Check.Null(options);
        Check.Null(builder);
        Check.Argument(message.CategoryName != null, message);

        var (lvl, r, g, b) = message.LogLevel switch
        {
            LogLevel.Trace => ("TRC", 127, 0, 127),
            LogLevel.Debug => ("DBG", 0, 127, 255),
            LogLevel.Information => ("INF", 255, 255, 255),
            LogLevel.Warning => ("WRN", 255, 255, 0),
            LogLevel.Error => ("ERR", 255, 63, 0),
            LogLevel.Critical => ("CRT", 255, 0, 0),
            _ => throw new ArgumentException(message: null, nameof(message)),
        };

        Decorator Decorate(byte r, byte g, byte b)
        {
            return options.UseColors ? new(builder, Color.FromArgb(byte.MaxValue, r, g, b)) : default;
        }

        _ = builder.Print("[");

        using (_ = Decorate(127, 127, 127))
            _ = builder.Print(CultureInfo.InvariantCulture, $"{message.Timestamp:HH:mm:ss.fff}");

        _ = builder.Print("][");

        using (_ = Decorate((byte)r, (byte)g, (byte)b))
            _ = builder.Print(lvl);

        _ = builder.Print("][");

        using (_ = Decorate(233, 233, 233))
            _ = builder.Print(message.CategoryName);

        _ = builder.Print("][");

        using (_ = Decorate(0, 155, 155))
            _ = builder.Print(message.EventId);

        _ = builder.Print("] ");

        var single = options.SingleLine;
        var msg = message.Message;

        if (single)
            msg = msg.ReplaceLineEndings(" ");

        var hasMsg = !string.IsNullOrWhiteSpace(msg);

        if (hasMsg)
            _ = builder.Print(msg);

        if (message.Exception is Exception e)
        {
            if (hasMsg)
                _ = single ? builder.Space() : builder.PrintLine();

            var excMsg = e.ToString();

            if (single)
                excMsg = excMsg.ReplaceLineEndings(" ");

            _ = builder.Print(excMsg);
        }
    }

    public static void Systemd(TerminalLoggerOptions options, ControlBuilder builder, in TerminalLoggerMessage message)
    {
        Check.Null(options);
        Check.Null(builder);
        Check.Argument(message.CategoryName != null, message);

        var lvl = message.LogLevel switch
        {
            LogLevel.Trace => "<7>",
            LogLevel.Debug => "<7>",
            LogLevel.Information => "<6>",
            LogLevel.Warning => "<4>",
            LogLevel.Error => "<3>",
            LogLevel.Critical => "<2>",
            _ => throw new ArgumentException(message: null, nameof(message)),
        };

        var culture = CultureInfo.InvariantCulture;

        _ = builder
            .Print(lvl)
            .Print(culture, $"[{message.Timestamp:HH:mm:ss.fff}]")
            .Print(culture, $"[{message.CategoryName}]")
            .Print(culture, $"[{message.EventId}] ");

        var msg = message.Message.ReplaceLineEndings(" ");
        var hasMsg = !string.IsNullOrWhiteSpace(msg);

        if (hasMsg)
            _ = builder.Print(msg);

        if (message.Exception is Exception e)
        {
            if (hasMsg)
                _ = builder.Space();

            _ = builder.Print(e.ToString().ReplaceLineEndings(" "));
        }
    }
}
