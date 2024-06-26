// SPDX-License-Identifier: 0BSD

namespace Vezel.Cathode.Extensions.Logging;

public static class TerminalLoggingBuilderExtensions
{
    [UnconditionalSuppressMessage("", "IL2026")]
    [UnconditionalSuppressMessage("", "IL3050")]
    public static ILoggingBuilder AddTerminal(
        this ILoggingBuilder builder, Action<TerminalLoggerOptions>? configureOptions = null)
    {
        Check.Null(builder);

        builder.AddConfiguration();
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, TerminalLoggerProvider>());
        LoggerProviderOptions.RegisterProviderOptions<TerminalLoggerOptions, TerminalLoggerProvider>(builder.Services);

        if (configureOptions != null)
            _ = builder.Services.Configure(configureOptions);

        return builder;
    }
}
