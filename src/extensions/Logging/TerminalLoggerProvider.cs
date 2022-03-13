using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cathode.Extensions.Logging;

[ProviderAlias(nameof(Terminal))]
public sealed class TerminalLoggerProvider : ILoggerProvider, ISupportExternalScope
{
    readonly ConcurrentDictionary<string, TerminalLogger> _loggers = new();

    readonly IOptionsMonitor<TerminalLoggerOptions> _options;

    readonly TerminalLoggerProcessor _processor;

    IExternalScopeProvider _scopeProvider = NullExternalScopeProvider.Instance;

    public TerminalLoggerProvider(IOptionsMonitor<TerminalLoggerOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = options;
        _processor = new(options.CurrentValue.LogQueueSize);
    }

    public void Dispose()
    {
        _processor.Dispose();
    }

    public ILogger CreateLogger(string categoryName)
    {
        ArgumentNullException.ThrowIfNull(categoryName);

        return _loggers.GetOrAdd(categoryName, name => new(name, _options, _processor, _scopeProvider));
    }

    public void SetScopeProvider(IExternalScopeProvider scopeProvider)
    {
        ArgumentNullException.ThrowIfNull(scopeProvider);

        _scopeProvider = scopeProvider;

        foreach (var (_, logger) in _loggers)
            logger.ScopeProvider = _scopeProvider;
    }
}