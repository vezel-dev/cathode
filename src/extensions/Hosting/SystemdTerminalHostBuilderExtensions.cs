// SPDX-License-Identifier: 0BSD

using Vezel.Cathode.Extensions.Logging;

namespace Vezel.Cathode.Extensions.Hosting;

public static class SystemdTerminalHostBuilderExtensions
{
    public static IHostBuilder UseTerminalSystemd(this IHostBuilder hostBuilder)
    {
        Check.Null(hostBuilder);

        if (SystemdHelpers.IsSystemdService())
            _ = hostBuilder.ConfigureServices((ctx, services) =>
                services
                    .Configure<TerminalLoggerOptions>(opts => opts.Writer = TerminalLoggerWriters.Systemd)
                    .AddSingleton<ISystemdNotifier, SystemdNotifier>()
                    .AddSingleton<IHostLifetime, SystemdLifetime>());

        return hostBuilder;
    }
}
