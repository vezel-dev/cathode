using System;

namespace Microsoft.Extensions.Logging
{
    sealed class TerminalLogger : ILogger
    {
        public TerminalLoggerOptions Options { get; set; }

        public IExternalScopeProvider ScopeProvider { get; set; }

        readonly TerminalLoggerProcessor _processor;

        public TerminalLogger(TerminalLoggerOptions options, IExternalScopeProvider scopeProvider,
            TerminalLoggerProcessor processor)
        {
            Options = options;
            ScopeProvider = scopeProvider;
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

            // TODO
            throw new NotImplementedException();
        }
    }
}
