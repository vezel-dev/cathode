namespace Vezel.Cathode.Extensions.Logging;

sealed class NullExternalScopeProvider : IExternalScopeProvider
{
    public static NullExternalScopeProvider Instance { get; } = new();

    NullExternalScopeProvider()
    {
    }

    public void ForEachScope<TState>(Action<object, TState> callback, TState state)
    {
    }

    public IDisposable Push(object? state)
    {
        return NullScope.Instance;
    }
}
