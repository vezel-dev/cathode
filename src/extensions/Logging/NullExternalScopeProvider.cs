namespace Vezel.Cathode.Extensions.Logging;

internal sealed class NullExternalScopeProvider : IExternalScopeProvider
{
    public static NullExternalScopeProvider Instance { get; } = new();

    private NullExternalScopeProvider()
    {
    }

    public void ForEachScope<TState>(Action<object?, TState> callback, TState state)
    {
    }

    public IDisposable Push(object? state)
    {
        return NullScope.Instance;
    }
}
