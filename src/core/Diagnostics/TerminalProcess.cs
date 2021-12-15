namespace System.Diagnostics;

public sealed class TerminalProcess
{
    // TODO: Support more of Process's API surface (threads, modules, memory, etc).

    public event Action<string?>? StandardOutReceived;

    public event Action<string?>? StandardErrorReceived;

    public event Action? Exited;

    public static TerminalProcess CurrentProcess { get; } = new(Process.GetCurrentProcess(), false);

    public SafeHandle Handle => _process.SafeHandle;

    public int Id => _process.Id;

    public string Name => _process.ProcessName;

    public DateTime StartTime => _process.StartTime;

    public bool IsResponding => _process.Responding;

    public bool HasExited => _process.HasExited;

    public int ExitCode => _process.ExitCode;

    public DateTime ExitTime => _process.ExitTime;

    public StreamWriter StandardIn => _process.StandardInput;

    public StreamReader StandardOut => _process.StandardOutput;

    public StreamReader StandardError => _process.StandardError;

    readonly Process _process;

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
                SystemVirtualTerminal.Instance.ReapProcess(this);

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
            SystemVirtualTerminal.Instance.StartProcess(() =>
            {
                _ = proc.Start();

                return wrapper;
            });
        }
        else
            _ = proc.Start();

        return wrapper;
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
