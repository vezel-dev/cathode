namespace Cathode.Threading;

static class SynchronizationExtensions
{
    public readonly struct LockDisposable : IDisposable
    {
        readonly SemaphoreSlim _semaphore;

        public LockDisposable(SemaphoreSlim semaphore)
        {
            _semaphore = semaphore;
        }

        public void Dispose()
        {
            _ = _semaphore.Release();
        }
    }

    public static LockDisposable Enter(this SemaphoreSlim semaphore, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(semaphore);

        semaphore.Wait(cancellationToken);

        return new(semaphore);
    }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
    public static async ValueTask<LockDisposable> EnterAsync(
        this SemaphoreSlim semaphore,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(semaphore);

        await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

        return new(semaphore);
    }
}
