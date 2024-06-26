// SPDX-License-Identifier: 0BSD

namespace Vezel.Cathode.Threading;

[SuppressMessage("", "CA1001")]
internal sealed class AsyncReaderWriterLockSlim
{
    // This is a simple, async-compatible, writer-biased reader/writer lock. The assumption is that write sections will
    // be considerably less frequent than read sections and also very short-lived, so new readers will be forced to spin
    // and yield when the write lock is held.
    //
    // Due to the nature of async/await, this class has no notion of threads. So, read lock recursion is supported while
    // the write lock is not held, but attempting to enter the read lock while holding the write lock will deadlock.

    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private int _state;

    private bool EnterReadLockCore(CancellationToken cancellationToken)
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
        var wait = default(SpinWait);

        while (!EnterReadLockCore(cancellationToken))
            wait.SpinOnce();
    }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder))]
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

    private bool EnterWriteLockCore(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Interlocked.CompareExchange(ref _state, -1, 0) == 0;
    }

    public void EnterWriteLock(CancellationToken cancellationToken = default)
    {
        _semaphore.Wait(cancellationToken);

        var wait = default(SpinWait);

        // Wait for readers to exit the lock.
        while (!EnterWriteLockCore(cancellationToken))
            wait.SpinOnce();
    }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder))]
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
            // This means that there were unbalanced EnterWriteLock/ExitWriteLock calls.
            throw new SynchronizationLockException();
        }

        Volatile.Write(ref _state, 0);
    }
}
