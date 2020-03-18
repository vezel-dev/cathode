using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting
{
    public static class TerminalHostBuilderExtensions
    {
        public static IHostBuilder UseTerminalLifetime(this IHostBuilder hostBuilder,
            Action<TerminalLifetimeOptions>? configureOptions = null)
        {
            return (hostBuilder ?? throw new ArgumentNullException(nameof(hostBuilder))).ConfigureServices(services =>
            {
                _ = services.AddSingleton<IHostLifetime, TerminalLifetime>();

                if (configureOptions != null)
                    _ = services.Configure(configureOptions);
            });
        }

        public static Task RunTerminalAsync(this IHostBuilder hostBuilder,
            Action<TerminalLifetimeOptions>? configureOptions = null, CancellationToken cancellationToken = default)
        {
            return hostBuilder.UseTerminalLifetime(configureOptions).Build().RunAsync(cancellationToken);
        }
    }
}
