using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Microsoft.Extensions.Logging.Terminal
{
    sealed class TerminalLoggerProcessor : IDisposable
    {
        const int QueueSize = 4096;

        public TerminalLoggerOptions Options { get; set; }

        readonly BlockingCollection<TerminalLoggerEntry> _queue =
            new BlockingCollection<TerminalLoggerEntry>(QueueSize);

        readonly Thread _thread;

        public TerminalLoggerProcessor(TerminalLoggerOptions options)
        {
            Options = options;
            _thread = TerminalUtility.StartThread("Terminal Log Processor", () =>
            {
                try
                {
                    foreach (var msg in _queue.GetConsumingEnumerable())
                        Write(msg);
                }
                finally
                {
                    // If for some reason we encounter an exception in this
                    // thread, we need to mark the queue as completed so that
                    // subsequent writes will at least happen in Enqueue.
                    _queue.CompleteAdding();
                }
            });
        }

        public void Enqueue(in TerminalLoggerEntry entry)
        {
            try
            {
                _queue.Add(entry);
            }
            catch (InvalidOperationException)
            {
                // The processor thread is gone, so just write it directly.
                Write(entry);
            }
        }

        void Write(in TerminalLoggerEntry entry)
        {
            Options.Writer(Options, entry.LogLevel >= Options.LogToStandardErrorThreshold ?
                System.Terminal.StdError : System.Terminal.StdOut, entry);
        }

        public void Dispose()
        {
            _queue.CompleteAdding();

            _ = _thread.Join(1500);
        }
    }
}
