using Cathode.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;

namespace Cathode.Extensions.Hosting;

public static class TerminalHost
{
    public static IHostBuilder CreateDefaultBuilder(string[]? args = null)
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
                [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026")]
                static T GetConfigurationValue<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
                    HostBuilderContext context, string key, T defaultValue)
                {
                    return context.Configuration.GetValue(key, defaultValue);
                }

                var reload = GetConfigurationValue(ctx, "hostBuilder:reloadConfigOnChange", true);
                var env = ctx.HostingEnvironment;

                _ = cfg
                    .AddJsonFile("appsettings.json", true, reload)
                    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, reload);

                if (env.IsDevelopment() && !string.IsNullOrEmpty(env.ApplicationName))
                {
                    var asm = Assembly.Load(new AssemblyName(env.ApplicationName));

                    if (asm != null)
                        _ = cfg.AddUserSecrets(asm, true, reload);
                }

                _ = cfg.AddEnvironmentVariables();

                if (args != null)
                    _ = cfg.AddCommandLine(args);
            })
            .ConfigureLogging((ctx, log) =>
            {
                var win = OperatingSystem.IsWindows();

                if (win)
                    _ = log.AddFilter<EventLogLoggerProvider>(lvl => lvl >= LogLevel.Warning);

                _ = log
                    .AddConfiguration(ctx.Configuration.GetSection("Logging"))
                    .AddTerminal()
                    .AddDebug()
                    .AddEventSourceLogger();

                if (win)
                    _ = log.AddEventLog();

                _ = log.Configure(opts =>
                    opts.ActivityTrackingOptions =
                        ActivityTrackingOptions.SpanId |
                        ActivityTrackingOptions.TraceId |
                        ActivityTrackingOptions.ParentId);
            })
            .UseDefaultServiceProvider((ctx, opts) =>
            {
                var dev = ctx.HostingEnvironment.IsDevelopment();

                opts.ValidateOnBuild = dev;
                opts.ValidateScopes = dev;
            });
    }
}
