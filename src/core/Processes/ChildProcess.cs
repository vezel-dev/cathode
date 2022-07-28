namespace Vezel.Cathode.Processes;

[SuppressMessage("", "RS0030")]
[SuppressMessage("", "CA1001")]
public sealed class ChildProcess
{
    public int Id { get; }

    public ChildProcessWriter StandardIn =>
        _in ?? throw new InvalidOperationException("Standard input is not redirected.");

    public ChildProcessReader StandardOut =>
        _out ?? throw new InvalidOperationException("Standard output is not redirected.");

    public ChildProcessReader StandardError =>
        _error ?? throw new InvalidOperationException("Standard error is not redirected.");

    public Task<int> Completion { get; }

    private readonly Process _process;

    private readonly ChildProcessWriter? _in;

    private readonly ChildProcessReader? _out;

    private readonly ChildProcessReader? _error;

    private readonly TaskCompletionSource<int> _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private readonly TaskCompletionSource _exited = new(TaskCreationOptions.RunContinuationsAsynchronously);

    [SuppressMessage("", "CA1031")]
    internal ChildProcess(ChildProcessBuilder builder)
    {
        var (redirectIn, redirectOut, redirectError) =
            (builder.RedirectStandardIn, builder.RedirectStandardOut, builder.RedirectStandardError);

        var info = new ProcessStartInfo()
        {
            FileName = builder.FileName,
            WorkingDirectory = builder.WorkingDirectory,
            CreateNoWindow = !builder.CreateWindow,
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

        _process = new Process
        {
            StartInfo = info,
            EnableRaisingEvents = true,
        };

        var ctr = default(CancellationTokenRegistration);
        var throwOnError = builder.ThrowOnError;
        var terminal = !(redirectIn && redirectOut && redirectError);

        _process.Exited += (_, _) =>
        {
            ctr.Dispose();

            var code = _process.ExitCode;

            _ = throwOnError && code != 0 ?
                _completion.TrySetException(new ChildProcessException($"Process exited with code {code}.", code)) :
                _completion.TrySetResult(code);

            if (terminal)
                Terminal.System.ReapProcess(this);

            _exited.SetResult();
        };

        // If the child process might use the terminal, start it under the raw mode lock since we only allow starting
        // non-redirected processes in cooked mode and we need to verify our current mode.
        if (terminal)
        {
            Terminal.System.StartProcess(() =>
            {
                _ = _process.Start();

                return this;
            });
        }
        else
            _ = _process.Start();

        Id = _process.Id;

        if (redirectIn)
            _in = new(_process.StandardInput);

        var tasks = new List<Task>(2);

        if (redirectOut)
            tasks.Add((_out = new(_process.StandardOutput, builder.StandardOutBufferSize)).Completion);

        if (redirectError)
            tasks.Add((_error = new(_process.StandardError, builder.StandardErrorBufferSize)).Completion);

        // We register the cancellation callback here, after it has started, so that we do not potentially kill the
        // process prior to or during startup.
        ctr = builder.CancellationToken.UnsafeRegister(
            static (state, token) => ((ChildProcess)state!)._completion.TrySetCanceled(token), this);

        Completion = Task.Run(async () =>
        {
            int code;

            try
            {
                code = await _completion.Task.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                try
                {
                    _process.Kill(true);
                }
                catch (Exception)
                {
                    // Even if killing the process tree somehow fails, there is nothing we can do about it here.
                }

                // Normally, _completion is completed from the Exited event handler. In the case of cancellation, we
                // complete it from the cancellation callback. This means that we have to wait for the Exited event
                // handler to run so that it becomes safe to dispose the process.
                await _exited.Task.ConfigureAwait(false);

                throw;
            }
            finally
            {
                // At this point, we know we are completely finished with the process.
                _process.Dispose();
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            return code;
        });
    }

    public static ChildProcess Run(string fileName, params string[] arguments)
    {
        return new ChildProcessBuilder()
            .WithFileName(fileName)
            .WithArguments(arguments)
            .Run();
    }

    public void Kill(bool entireProcessTree = true)
    {
        try
        {
            // TODO: Review exceptions that can be thrown here.
            _process.Kill(entireProcessTree);
        }
        catch (InvalidOperationException)
        {
            // The process is already gone.
        }

        _process.WaitForExit();
    }
}
