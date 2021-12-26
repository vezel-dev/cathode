namespace System;

public sealed class TerminalControl
{
    readonly AsyncReaderWriterLockSlim _lock = new();

    readonly AsyncLocal<object> _current = new();

    object? _controller;

    internal TerminalControl()
    {
    }

    public IDisposable Acquire(CancellationToken cancellationToken = default)
    {
        _lock.EnterWriteLock(cancellationToken);

        try
        {
            if (_controller != null)
                throw new InvalidOperationException("Terminal control has already been acquired.");

            _controller = _current.Value = new();
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        return new HeapDisposable<TerminalControl>(this, static c =>
        {
            c._lock.EnterWriteLock();

            c._controller = null;

            c._lock.ExitWriteLock();
        });
    }

    internal StackDisposable<TerminalControl> Guard()
    {
        _lock.EnterReadLock();

        if (_controller != null && _current.Value != _controller)
        {
            _lock.ExitReadLock();

            throw new InvalidOperationException("Caller does not have terminal control.");
        }

        return new(this, static c => c._lock.ExitReadLock());
    }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
    internal async ValueTask<StackDisposable<TerminalControl>> GuardAsync()
    {
        await _lock.EnterReadLockAsync().ConfigureAwait(false);

        if (_controller != null && _current.Value != _controller)
        {
            _lock.ExitReadLock();

            throw new InvalidOperationException("Caller does not have terminal control.");
        }

        return new(this, static c => c._lock.ExitReadLock());
    }
}
