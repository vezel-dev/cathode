// SPDX-License-Identifier: 0BSD

namespace Vezel.Cathode.Extensions.Logging;

[ProviderAlias("Terminal")]
public sealed class TerminalLoggerProvider : ILoggerProvider, ISupportExternalScope
{
    private readonly ConcurrentDictionary<string, TerminalLogger> _loggers = new();

    private readonly IOptionsMonitor<TerminalLoggerOptions> _options;

    private readonly TerminalLoggerProcessor _processor;

    private IExternalScopeProvider _scopeProvider = NullExternalScopeProvider.Instance;

    public TerminalLoggerProvider(IOptionsMonitor<TerminalLoggerOptions> options)
    {
        Check.Null(options);

        _options = options;
        _processor = new(options.CurrentValue.LogQueueSize);
    }

    public void Dispose()
    {
        _processor.Dispose();
    }

    public ILogger CreateLogger(string categoryName)
    {
        Check.Null(categoryName);

        return _loggers.GetOrAdd(categoryName, name => new(name, _options, _processor, _scopeProvider));
    }

    public void SetScopeProvider(IExternalScopeProvider scopeProvider)
    {
        Check.Null(scopeProvider);

        _scopeProvider = scopeProvider;

        foreach (var (_, logger) in _loggers)
            logger.ScopeProvider = _scopeProvider;
    }
}
