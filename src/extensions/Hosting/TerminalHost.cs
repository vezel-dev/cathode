using Vezel.Cathode.Extensions.Logging;

namespace Vezel.Cathode.Extensions.Hosting;

public static class TerminalHost
{
    [RequiresDynamicCode("Dependency injection may require generating code at runtime.")]
    public static IHostBuilder CreateDefaultBuilder(string[]? args = null)
    {
        var builder = new HostBuilder();
        var cwd = Environment.CurrentDirectory;

        if (!OperatingSystem.IsWindows() ||
            !string.Equals(
                cwd, Environment.GetFolderPath(Environment.SpecialFolder.System), StringComparison.OrdinalIgnoreCase))
            _ = builder.UseContentRoot(cwd);

        return builder
            .ConfigureHostConfiguration(cfg =>
            {
                _ = cfg.AddEnvironmentVariables("DOTNET_");

                if (args != null)
                    _ = cfg.AddCommandLine(args);
            })
            .ConfigureAppConfiguration((ctx, cfg) =>
            {
                [UnconditionalSuppressMessage("", "IL2026")]
                static T? GetValueOrDefault<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
                    HostBuilderContext context, string key, T defaultValue)
                {
                    return context.Configuration.GetValue(key, defaultValue);
                }

                var reload = GetValueOrDefault(ctx, "hostBuilder:reloadConfigOnChange", true);
                var env = ctx.HostingEnvironment;

                _ = cfg
                    .AddJsonFile("appsettings.json", optional: true, reload)
                    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reload);

                if (env.IsDevelopment() && !string.IsNullOrEmpty(env.ApplicationName))
                {
                    var asm = default(Assembly);

                    try
                    {
                        asm = Assembly.Load(new AssemblyName(env.ApplicationName));
                    }
                    catch (FileNotFoundException)
                    {
                        // Assembly not found.
                    }

                    if (asm != null)
                        _ = cfg.AddUserSecrets(asm, optional: true, reload);
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
