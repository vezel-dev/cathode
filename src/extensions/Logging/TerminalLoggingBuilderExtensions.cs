using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Configuration;

namespace Microsoft.Extensions.Logging
{
    public static class TerminalLoggingBuilderExtensions
    {
        public static ILoggingBuilder AddTerminal(this ILoggingBuilder builder,
            Action<TerminalLoggerOptions>? configureOptions = null)
        {
            builder.AddConfiguration();

            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, TerminalLoggerProvider>());
            LoggerProviderOptions.RegisterProviderOptions<TerminalLoggerOptions, TerminalLoggerProvider>(
                builder.Services);

            if (configureOptions != null)
                _ = builder.Services.Configure(configureOptions);

            return builder;
        }
    }
}
