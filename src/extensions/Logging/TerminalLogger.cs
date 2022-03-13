using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cathode.Extensions.Logging;

sealed class TerminalLogger : ILogger
{
    public IExternalScopeProvider ScopeProvider { get; set; }

    static readonly ThreadLocal<ControlBuilder> _builder = new(() => new());

    readonly string _name;

    readonly IOptionsMonitor<TerminalLoggerOptions> _options;

    readonly TerminalLoggerProcessor _processor;

    public TerminalLogger(
        string name,
        IOptionsMonitor<TerminalLoggerOptions> options,
        TerminalLoggerProcessor processor,
        IExternalScopeProvider scopeProvider)
    {
        _name = name;
        _options = options;
        _processor = processor;
        ScopeProvider = scopeProvider;
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        return ScopeProvider.Push(state) ?? NullScope.Instance;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel != LogLevel.None;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        ArgumentNullException.ThrowIfNull(formatter);

        if (!IsEnabled(logLevel))
            return;

        var msg = formatter(state, exception);

        if (string.IsNullOrWhiteSpace(msg) && exception == null)
            return;

        var opts = _options.CurrentValue;
        var now = opts.UseUtcTimestamp ? DateTime.UtcNow : DateTime.Now;
        var cb = _builder.Value!;

        try
        {
            opts.Writer(opts, cb, new(now, logLevel, _name, eventId, msg, exception));

            _ = cb.PrintLine();

            var span = cb.Span;
            var array = ArrayPool<char>.Shared.Rent(span.Length);

            span.CopyTo(array);

            _processor.Enqueue(new(
                array.AsMemory(..span.Length),
                logLevel >= opts.LogToStandardErrorThreshold ? Terminal.StandardError : Terminal.StandardOut));
        }
        finally
        {
            cb.Clear();
        }
    }
}