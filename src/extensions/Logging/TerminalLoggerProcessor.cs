// SPDX-License-Identifier: 0BSD

namespace Vezel.Cathode.Extensions.Logging;

internal sealed class TerminalLoggerProcessor : IDisposable
{
    private readonly BlockingCollection<TerminalLoggerEntry> _queue;

    private readonly Thread _thread;

    [SuppressMessage("", "CA1031")]
    public TerminalLoggerProcessor(int queueSize)
    {
        _queue = new(queueSize);
        _thread = new Thread(() =>
        {
            try
            {
                foreach (var msg in _queue.GetConsumingEnumerable())
                    Write(msg);
            }
            catch (Exception)
            {
                // The writer method has failed somehow. Ensure that subsequent writes at least happen in Enqueue.

                try
                {
                    _queue.CompleteAdding();
                }
                catch (ObjectDisposedException)
                {
                }
            }
        })
        {
            Name = "Terminal Log Processor",
            IsBackground = true,
        };

        _thread.Start();
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

    public void Enqueue(TerminalLoggerEntry entry)
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

    private static void Write(TerminalLoggerEntry entry)
    {
        entry.Writer.Write(entry.Message.Span);

        _ = MemoryMarshal.TryGetArray(entry.Message, out var seg);

        ArrayPool<char>.Shared.Return(seg.Array!);
    }
}
