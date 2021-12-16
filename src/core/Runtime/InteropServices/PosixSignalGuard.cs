namespace System.Runtime.InteropServices;

sealed class PosixSignalGuard : IDisposable
{
    public bool Signaled { get; private set; }

    readonly PosixSignalRegistration _registration;

    public PosixSignalGuard(PosixSignal signal)
    {
        _registration = PosixSignalRegistration.Create(signal, _ => Signaled = true);
    }

    ~PosixSignalGuard()
    {
        Dispose();
    }

    public void Dispose()
    {
        _registration.Dispose();

        GC.SuppressFinalize(this);
    }
}
