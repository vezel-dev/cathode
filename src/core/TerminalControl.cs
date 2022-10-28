namespace Vezel.Cathode;

public sealed class TerminalControl
{
    public sealed class AcquireDisposable : IDisposable
    {
        private readonly TerminalControl _control;

        internal AcquireDisposable(TerminalControl control)
        {
            _control = control;
        }

        public void Dispose()
        {
            _control._lock.EnterWriteLock();

            _control._controller = null;

            _control._lock.ExitWriteLock();
        }
    }

    internal readonly struct GuardDisposable : IDisposable
    {
        private readonly TerminalControl _control;

        public GuardDisposable(TerminalControl control)
        {
            _control = control;
        }

        public void Dispose()
        {
            _control._lock.ExitReadLock();
        }
    }

    private readonly AsyncReaderWriterLockSlim _lock = new();

    private readonly AsyncLocal<object> _current = new();

    private object? _controller;

    internal TerminalControl()
    {
    }

    public AcquireDisposable Acquire(CancellationToken cancellationToken = default)
    {
        _lock.EnterWriteLock(cancellationToken);

        try
        {
            Check.Operation(_controller == null, $"Terminal control has already been acquired.");

            _controller = _current.Value = new();
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        return new(this);
    }

    internal GuardDisposable Guard()
    {
        _lock.EnterReadLock();

        if (_controller != null && _current.Value != _controller)
        {
            _lock.ExitReadLock();

            throw new InvalidOperationException("Caller does not have terminal control.");
        }

        return new(this);
    }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
    internal async ValueTask<GuardDisposable> GuardAsync()
    {
        await _lock.EnterReadLockAsync().ConfigureAwait(false);

        if (_controller != null && _current.Value != _controller)
        {
            _lock.ExitReadLock();

            throw new InvalidOperationException("Caller does not have terminal control.");
        }

        return new(this);
    }
}
