namespace Vezel.Cathode.Hosting;

public sealed class ProgramContext
{
    public event Action<Exception>? UnhandledException;

    public event Action? ProcessExiting;

    public ReadOnlyMemory<string> Arguments { get; }

    public int ExitCode { get; set; }

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
