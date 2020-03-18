using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;

namespace Microsoft.Extensions.Hosting
{
    public static class TerminalHost
    {
        public static IHostBuilder CreateDefaultBuilder(string[]? args)
        {
            return new HostBuilder()
                .UseContentRoot(Environment.CurrentDirectory)
                .ConfigureHostConfiguration(cfg =>
                {
                    _ = cfg.AddEnvironmentVariables(prefix: "DOTNET_");

                    if (args != null)
                        _ = cfg.AddCommandLine(args);
                })
                .ConfigureAppConfiguration((ctx, cfg) =>
                {
                    var env = ctx.HostingEnvironment;

                    _ = cfg
                        .AddJsonFile("appsettings.json", true, true)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true);

                    if (env.IsDevelopment() && !string.IsNullOrEmpty(env.ApplicationName))
                    {
                        var asm = Assembly.Load(new AssemblyName(env.ApplicationName));

                        if (asm != null)
                            _ = cfg.AddUserSecrets(asm, true);
                    }

                    _ = cfg.AddEnvironmentVariables();

                    if (args != null)
                        _ = cfg.AddCommandLine(args);
                })
                .ConfigureLogging((ctx, log) =>
                {
                    var win = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

                    if (win)
                        _ = log.AddFilter<EventLogLoggerProvider>(lvl => lvl >= LogLevel.Warning);

                    _ = log
                        .AddConfiguration(ctx.Configuration.GetSection("Logging"))
                        .AddTerminal()
                        .AddDebug()
                        .AddEventSourceLogger();

                    if (win)
                        _ = log.AddEventLog();
                })
                .UseDefaultServiceProvider((ctx, opts) =>
                {
                    var dev = ctx.HostingEnvironment.IsDevelopment();

                    opts.ValidateOnBuild = dev;
                    opts.ValidateScopes = dev;
                });
        }
    }
}
