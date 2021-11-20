namespace Microsoft.Extensions.Logging.Terminal;

sealed class TerminalLoggerProcessor : IDisposable
{
    public TerminalLoggerOptions Options { get; set; }

    readonly BlockingCollection<TerminalLoggerEntry> _queue;

    readonly Thread _thread;

    [SuppressMessage("Design", "CA1031")]
    public TerminalLoggerProcessor(TerminalLoggerOptions options)
    {
        Options = options;
        _queue = new(options.LogQueueSize);
        _thread = TerminalUtility.StartThread("Terminal Log Processor", () =>
        {
            try
            {
                foreach (var msg in _queue.GetConsumingEnumerable())
                    Write(msg);
            }
            catch (Exception)
            {
                // The writer method has failed somehow. Ensure that
                // subsequent writes at least happen in Enqueue.

                try
                {
                    _queue.CompleteAdding();
                }
                catch (ObjectDisposedException)
                {
                }
            }
        });
    }

    public void Enqueue(in TerminalLoggerEntry entry)
    {
        try
        {
            _queue.Add(entry);
        }
        catch (Exception ex) when (ex is InvalidOperationException or ObjectDisposedException)
        {
            // The processor thread is gone, so just write it directly.
            Write(entry);
        }
    }

    void Write(in TerminalLoggerEntry entry)
    {
        Options.Writer(
            Options,
            entry.LogLevel >= Options.LogToStandardErrorThreshold ? System.Terminal.StdError : System.Terminal.StdOut,
            entry);
    }

    public void Dispose()
    {
        try
        {
            _queue.CompleteAdding();
        }
        catch (ObjectDisposedException)
        {
            // It might already be completed due to a writer method failure.
        }

        _queue.Dispose();

        _ = _thread.Join(1500);
    }
}
