namespace System.Threading;

sealed class AsyncReaderWriterLock
{
    readonly SemaphoreSlim _semaphore = new(1, 1);

    int _state;

    bool EnterReadLockCore(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var old = Volatile.Read(ref _state);

        // Give priority to writers.
        if (_semaphore.CurrentCount == 0 || old < 0)
            return false;

        // This could fail if a writer or another reader managed to modify the lock state in the meantime.
        return Interlocked.CompareExchange(ref _state, old + 1, old) == old;
    }

    public void EnterReadLock(CancellationToken cancellationToken = default)
    {
        var wait = new SpinWait();

        while (!EnterReadLockCore(cancellationToken))
            wait.SpinOnce();
    }

    public async ValueTask EnterReadLockAsync(CancellationToken cancellationToken = default)
    {
        // Try synchronously once and then fall back to the thread pool.
        if (EnterReadLockCore(cancellationToken))
            return;

        await Task.Run(
            async () =>
            {
                while (!EnterReadLockCore(cancellationToken))
                    await Task.Yield();
            },
            cancellationToken).ConfigureAwait(false);
    }

    public void ExitReadLock()
    {
        // A negative value means that there were unbalanced EnterReadLock/ExitReadLock calls.
        if (Interlocked.Decrement(ref _state) < 0)
            throw new SynchronizationLockException();
    }

    bool EnterWriteLockCore(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Interlocked.CompareExchange(ref _state, -1, 0) == 0;
    }

    public void EnterWriteLock(CancellationToken cancellationToken = default)
    {
        _semaphore.Wait(cancellationToken);

        var wait = new SpinWait();

        // Wait for readers to exit the lock.
        while (!EnterWriteLockCore(cancellationToken))
            wait.SpinOnce();
    }

    public async ValueTask EnterWriteLockAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

        // Try synchronously once and then fall back to the thread pool.
        if (EnterWriteLockCore(cancellationToken))
            return;

        await Task.Run(
            async () =>
            {
                // Wait for readers to exit the lock.
                while (!EnterWriteLockCore(cancellationToken))
                    await Task.Yield();
            },
            cancellationToken).ConfigureAwait(false);
    }

    public void ExitWriteLock()
    {
        try
        {
            _ = _semaphore.Release();
        }
        catch (SemaphoreFullException)
        {
            throw new SynchronizationLockException();
        }

        Volatile.Write(ref _state, 0);
    }
}
