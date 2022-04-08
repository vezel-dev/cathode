namespace Vezel.Cathode.Extensions.Logging;

sealed class NullScope : IDisposable
{
    public static NullScope Instance { get; } = new();

    NullScope()
    {
    }

    public void Dispose()
    {
    }
}
