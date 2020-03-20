using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.Hosting
{
    public sealed class TerminalLifetime : IHostLifetime, IDisposable
    {
        readonly ManualResetEventSlim _disposeEvent = new ManualResetEventSlim(false);

        readonly TerminalLifetimeOptions _options;

        readonly IHostEnvironment _environment;

        readonly IHostApplicationLifetime _applicationLifetime;

        readonly HostOptions _hostOptions;

        readonly ILogger _logger;

        CancellationTokenRegistration _started;

        CancellationTokenRegistration _stopping;

        public TerminalLifetime(IOptions<TerminalLifetimeOptions> options, IHostEnvironment environment,
            IHostApplicationLifetime applicationLifetime, IOptions<HostOptions> hostOptions)
            : this(options, environment, applicationLifetime, hostOptions, NullLoggerFactory.Instance)
        {
        }

        public TerminalLifetime(IOptions<TerminalLifetimeOptions> options, IHostEnvironment environment,
            IHostApplicationLifetime applicationLifetime, IOptions<HostOptions> hostOptions,
            ILoggerFactory loggerFactory)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _applicationLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));
            _hostOptions = hostOptions?.Value ?? throw new ArgumentNullException(nameof(hostOptions));
            _logger = loggerFactory?.CreateLogger("Microsoft.Hosting.Lifetime") ??
                throw new ArgumentNullException(nameof(loggerFactory));
        }

        public void Dispose()
        {
            _disposeEvent.Set();

            AppDomain.CurrentDomain.ProcessExit -= OnProcessExit;
            Terminal.BreakSignal -= OnBreakSignal;

            _stopping.Dispose();
            _started.Dispose();
        }

        void OnBreakSignal(object? sender, TerminalBreakSignalEventArgs e)
        {
            // We will shut down gracefully.
            e.Cancel = true;

            _applicationLifetime.StopApplication();
        }

        void OnProcessExit(object? sender, EventArgs e)
        {
            _applicationLifetime.StopApplication();

            if (!_disposeEvent.Wait(_hostOptions.ShutdownTimeout))
                _logger.LogInformation("Waiting for the host to be disposed; this is taking longer than expected.");

            _disposeEvent.Wait();

            // CoreCLR sets a non-zero exit code for SIGTERM. We shut down gracefully, so revert it.
            Environment.ExitCode = 0;
        }

        public Task WaitForStartAsync(CancellationToken cancellationToken)
        {
            if (_options.Title is string t)
                Terminal.Title = t;

            if (!_options.SuppressStatusMessages)
            {
                _started = _applicationLifetime.ApplicationStarted.Register(() =>
                {
                    _logger.LogInformation("Application started. Press Ctrl+C to shut down.");
                    _logger.LogInformation("Hosting environment: {envName}", _environment.EnvironmentName);
                    _logger.LogInformation("Content root path: {contentRoot}", _environment.ContentRootPath);
                });

                _stopping = _applicationLifetime.ApplicationStopping.Register(() =>
                    _logger.LogInformation("Application is shutting down..."));
            }

            Terminal.BreakSignal += OnBreakSignal;
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
