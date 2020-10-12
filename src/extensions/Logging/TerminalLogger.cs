using System;

namespace Microsoft.Extensions.Logging.Terminal
{
    sealed class TerminalLogger : ILogger
    {
        public IExternalScopeProvider ScopeProvider { get; set; }

        readonly string _name;

        readonly TerminalLoggerProcessor _processor;

        public TerminalLogger(IExternalScopeProvider scopeProvider, string name, TerminalLoggerProcessor processor)
        {
            ScopeProvider = scopeProvider;
            _name = name;
            _processor = processor;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return ScopeProvider.Push(state) ?? NullScope.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string?> formatter)
        {
            _ = formatter ?? throw new ArgumentNullException(nameof(formatter));

            if (!IsEnabled(logLevel))
                return;

            var now = _processor.Options.UseUtcTimestamp ? DateTime.UtcNow : DateTime.Now;
            var msg = formatter(state, exception);

            if (!string.IsNullOrEmpty(msg) || exception != null)
                _processor.Enqueue(new(now, logLevel, _name, eventId, msg, exception));
        }
    }
}
