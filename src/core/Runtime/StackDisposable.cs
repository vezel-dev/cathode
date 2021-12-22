namespace System.Runtime;

readonly struct StackDisposable<T> : IDisposable
{
    readonly T _state;

    readonly Action<T> _action;

    public StackDisposable(T state, Action<T> action)
    {
        _state = state;
        _action = action;
    }

    public void Dispose()
    {
        _action(_state);
    }
}
