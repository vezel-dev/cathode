namespace System.Diagnostics;

public sealed class TerminalProcess
{
    // TODO: Review possible exceptions throughout and sanitize.

    public event Action<string?>? StandardOutReceived;

    public event Action<string?>? StandardErrorReceived;

    public event Action? Exited;

    public static TerminalProcess CurrentProcess { get; } = new(Process.GetCurrentProcess(), false);

    public SafeHandle Handle => _process.SafeHandle;

    public int Id => _process.Id;

    public string Name => _process.ProcessName;

    public int SessionId => _process.SessionId;

    public DateTime StartTime => _process.StartTime;

    public DateTime ExitTime => _process.ExitTime;

    public bool HasExited => _process.HasExited;

    public int ExitCode => _process.ExitCode;

    public TimeSpan UserTime => _process.UserProcessorTime;

    public TimeSpan KernelTime => _process.PrivilegedProcessorTime;

    public TimeSpan TotalTime => _process.TotalProcessorTime;

    public StreamWriter StandardIn => _process.StandardInput;

    public StreamReader StandardOut => _process.StandardOutput;

    public StreamReader StandardError => _process.StandardError;

    public TerminalProcessModule? MainModule
    {
        get
        {
            lock (_lock)
                return _main ??= _process.MainModule is ProcessModule m ? new(_process.MainModule) : null;
        }
    }

    public ImmutableArray<TerminalProcessModule> Modules
    {
        get
        {
            lock (_lock)
            {
                if (!_modules.IsDefault)
                    return _modules;

                var modules = _process.Modules;
                var builder = ImmutableArray.CreateBuilder<TerminalProcessModule>(modules.Count);

                foreach (ProcessModule module in modules)
                    builder.Add(new(module));

                return _modules = builder.MoveToImmutable();
            }
        }
    }

    public ImmutableArray<TerminalProcessThread> Threads
    {
        get
        {
            lock (_lock)
            {
                if (!_threads.IsDefault)
                    return _threads;

                var threads = _process.Threads;
                var builder = ImmutableArray.CreateBuilder<TerminalProcessThread>(threads.Count);

                foreach (ProcessThread thread in threads)
                    builder.Add(new(thread));

                return _threads = builder.MoveToImmutable();
            }
        }
    }

    readonly object _lock = new();

    readonly Process _process;

    TerminalProcessModule? _main;

    ImmutableArray<TerminalProcessModule> _modules;

    ImmutableArray<TerminalProcessThread> _threads;

    TerminalProcess(Process process, bool reap)
    {
        _process = process;

        process.EnableRaisingEvents = true;

        process.OutputDataReceived += (_, e) => StandardOutReceived?.Invoke(e.Data);
        process.ErrorDataReceived += (_, e) => StandardErrorReceived?.Invoke(e.Data);

        process.Exited += (_, _) =>
        {
            // Do this before invoking the Exited event since any misbehaving event handlers might otherwise cause us to
            // not reap the process, leaving terminal configuration in an unknown state.
            if (reap)
                Terminal.System.ReapProcess(this);

            Exited?.Invoke();
        };
    }

    public static TerminalProcess[] GetProcesses()
    {
        return Process.GetProcesses().Select(p => new TerminalProcess(p, false)).ToArray();
    }

    public static TerminalProcess[] GetProcessesByName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        return Process.GetProcessesByName(name).Select(p => new TerminalProcess(p, false)).ToArray();
    }

    public static TerminalProcess GetProcessById(int id)
    {
        return new(Process.GetProcessById(id), false);
    }

    internal static TerminalProcess Start(TerminalProcessBuilder builder)
    {
        var (redirectIn, redirectOut, redirectError) =
            (builder.RedirectStandardIn, builder.RedirectStandardOut, builder.RedirectStandardError);

        var info = new ProcessStartInfo()
        {
            FileName = builder.FileName,
            WorkingDirectory = builder.WorkingDirectory,
            UseShellExecute = builder.UseShell,
            CreateNoWindow = builder.NoWindow,
            WindowStyle = builder.WindowStyle,
            RedirectStandardInput = redirectIn,
            RedirectStandardOutput = redirectOut,
            RedirectStandardError = redirectError,
        };

        foreach (var arg in builder.Arguments)
            info.ArgumentList.Add(arg);

        foreach (var (name, value) in builder.Variables)
            info.Environment.Add(name, value);

        // Setting the encodings without redirections causes exceptions.

        if (redirectIn)
            info.StandardInputEncoding = builder.StandardInEncoding;

        if (redirectOut)
            info.StandardOutputEncoding = builder.StandardOutEncoding;

        if (redirectError)
            info.StandardErrorEncoding = builder.StandardErrorEncoding;

        var proc = new Process
        {
            StartInfo = info,
        };

        var redirected = redirectIn && redirectOut && redirectError;
        var wrapper = new TerminalProcess(proc, !redirected);

        // If the child process might use the terminal, start it under the raw mode lock since we only allow starting
        // non-redirected processes in cooked mode and we need to verify our current mode.
        if (!redirected)
        {
            Terminal.System.StartProcess(() =>
            {
                _ = proc.Start();

                return wrapper;
            });
        }
        else
            _ = proc.Start();

        return wrapper;
    }

    public void Refresh()
    {
        lock (_lock)
        {
            _process.Refresh();

            _main = null;
            _modules = default;
            _threads = default;
        }
    }

    public void StartReadingStandardOut()
    {
        _process.BeginOutputReadLine();
    }

    public void StartReadingStandardError()
    {
        _process.BeginErrorReadLine();
    }

    public void StopReadingStandardOut()
    {
        _process.CancelOutputRead();
    }

    public void StopReadingStandardError()
    {
        _process.CancelErrorRead();
    }

    public void SendSignal(TerminalSignal signal)
    {
        Terminal.System.SignalProcess(this, signal);
    }

    public void Kill(bool entireProcessTree = false)
    {
        _process.Kill(entireProcessTree);
    }

    public bool WaitForExit(int millisecondsTimeout = Timeout.Infinite)
    {
        return _process.WaitForExit(millisecondsTimeout);
    }

    public Task WaitForExitAsync(CancellationToken cancellationToken = default)
    {
        return _process.WaitForExitAsync(cancellationToken);
    }
}
