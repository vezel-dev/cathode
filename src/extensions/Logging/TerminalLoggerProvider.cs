using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.Logging.Terminal
{
    [ProviderAlias(nameof(Terminal))]
    public sealed class TerminalLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        readonly ConcurrentDictionary<string, TerminalLogger> _loggers = new();

        readonly IOptionsMonitor<TerminalLoggerOptions> _options;

        readonly TerminalLoggerProcessor _processor;

        readonly IDisposable _reload;

        IExternalScopeProvider _scopeProvider = NullExternalScopeProvider.Instance;

        public TerminalLoggerProvider(IOptionsMonitor<TerminalLoggerOptions> options)
        {
            _ = options ?? throw new ArgumentNullException(nameof(options));

            _options = options;
            _processor = new(options.CurrentValue);
            _reload = options.OnChange(opts => _processor.Options = opts);
        }

        public void Dispose()
        {
            _reload.Dispose();
            _processor.Dispose();
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName ?? throw new ArgumentNullException(categoryName),
                name => new(_scopeProvider, name, _processor));
        }

        public void SetScopeProvider(IExternalScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider ?? throw new ArgumentNullException(nameof(scopeProvider));

            foreach (var logger in _loggers)
                logger.Value.ScopeProvider = _scopeProvider;
        }
    }
}
