namespace Vezel.Cathode.Hosting;

public sealed class ProgramContext
{
    public event Action<Exception>? UnhandledException;

    public event Action? ProcessExiting;

    public ReadOnlyMemory<string> Arguments { get; }

    public CancellationToken CancellationToken => _cancellationToken.Value;

    public int ExitCode { get; set; }

    private readonly Lazy<CancellationToken> _cancellationToken = new(() =>
    {
        // We create the token lazily when the caller wants it because hooking the Signaled event can have undesirable
        // side effects when running under a debugger.

        var cts = new CancellationTokenSource();

        Terminal.Signaled += ctx =>
        {
            // We just kind of assume that any event handlers that come after us will not flip it back to
            // false. Not much we can do in that case.
            ctx.Cancel = true;

            cts.Cancel();
        };

        return cts.Token;
    });

    internal ProgramContext(ReadOnlyMemory<string> arguments)
    {
        Arguments = arguments;
    }

    // The following methods must never throw.

    [SuppressMessage("", "CA1031")]
    internal void RaiseUnhandledException(Exception exception)
    {
        var ev = UnhandledException;

        if (ev != null)
        {
            foreach (var dg in ev.GetInvocationList())
            {
                try
                {
                    ((Action<Exception>)dg).Invoke(exception);
                }
                catch (Exception)
                {
                }
            }
        }
    }

    [SuppressMessage("", "CA1031")]
    internal void RaiseProcessExit()
    {
        var ev = ProcessExiting;

        if (ev != null)
        {
            foreach (var dg in ev.GetInvocationList())
            {
                try
                {
                    ((Action)dg).Invoke();
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
