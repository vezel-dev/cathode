namespace Cathode.Terminals.Unix;

sealed class PosixSignalGuard : IDisposable
{
    public bool Signaled => _signaled;

    readonly PosixSignalRegistration _registration;

    volatile bool _signaled;

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
