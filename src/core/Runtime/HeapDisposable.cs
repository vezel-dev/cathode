namespace System.Runtime;

sealed class HeapDisposable<T> : IAsyncDisposable, IDisposable
{
    readonly T _state;

    readonly Action<T>? _syncAction;

    readonly Func<T, ValueTask>? _asyncAction;

    public HeapDisposable(T state, Action<T> action)
    {
        _state = state;
        _syncAction = action;
    }

    public HeapDisposable(T state, Func<T, ValueTask> action)
    {
        _state = state;
        _asyncAction = action;
    }

    public ValueTask DisposeAsync()
    {
        return _asyncAction?.Invoke(_state) ?? default;
    }

    public void Dispose()
    {
        _syncAction?.Invoke(_state);
    }
}
