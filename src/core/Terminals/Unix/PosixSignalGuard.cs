namespace Vezel.Cathode.Terminals.Unix;

internal sealed class PosixSignalGuard : IDisposable
{
    public bool Signaled => _signaled;

    private readonly PosixSignalRegistration _registration;

    private volatile bool _signaled;

    public PosixSignalGuard(PosixSignal signal)
    {
        _registration = PosixSignalRegistration.Create(signal, _ => _signaled = true);
    }

    ~PosixSignalGuard()
    {
        Dispose();
    }

    public void Dispose()
    {
        _registration?.Dispose();

        GC.SuppressFinalize(this);
    }
}
