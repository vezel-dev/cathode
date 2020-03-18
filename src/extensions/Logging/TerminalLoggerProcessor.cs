using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Microsoft.Extensions.Logging
{
    sealed class TerminalLoggerProcessor : IDisposable
    {
        const int QueueSize = 4096;

        readonly BlockingCollection<TerminalLoggerMessage> _messages =
            new BlockingCollection<TerminalLoggerMessage>(QueueSize);

        readonly Thread _thread;

        public TerminalLoggerProcessor()
        {
            _thread = TerminalUtility.StartThread("Terminal Log Processor", () =>
            {
                try
                {
                    foreach (var msg in _messages.GetConsumingEnumerable())
                        Write(msg);
                }
                finally
                {
                    // If for some reason we encounter an exception in this
                    // thread, we need to mark the queue as completed so that
                    // subsequent writes will at least happen in Enqueue.
                    _messages.CompleteAdding();
                }
            });
        }

        void Write(in TerminalLoggerMessage message)
        {
            // TODO
            throw new NotImplementedException();
        }

        public void Enqueue(in TerminalLoggerMessage message)
        {
            try
            {
                _messages.Add(message);
            }
            catch (InvalidOperationException)
            {
                // The processor thread is gone, so just write it directly.
                Write(message);
            }
        }

        public void Dispose()
        {
            _messages.CompleteAdding();

            _ = _thread.Join(1500);
        }
    }
}
