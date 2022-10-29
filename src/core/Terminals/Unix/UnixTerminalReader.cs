using static Vezel.Cathode.Unix.UnixPInvoke;

namespace Vezel.Cathode.Terminals.Unix;

internal sealed class UnixTerminalReader : NativeTerminalReader<UnixVirtualTerminal, int>
{
    private readonly SemaphoreSlim _semaphore;

    private readonly UnixCancellationPipe _cancellationPipe;

    public UnixTerminalReader(
        UnixVirtualTerminal terminal,
        string name,
        int handle,
        UnixCancellationPipe cancellationPipe,
        SemaphoreSlim semaphore)
        : base(terminal, name, handle)
    {
        _semaphore = semaphore;
        _cancellationPipe = cancellationPipe;
    }

    protected override int ReadPartialCore(scoped Span<byte> buffer, CancellationToken cancellationToken)
    {
        using var guard = Terminal.Control.Guard();

        // If the descriptor is invalid, just present the illusion to the user that it has been redirected to /dev/null
        // or something along those lines, i.e. return EOF.
        if (buffer.IsEmpty || !IsValid)
            return 0;

        using (_semaphore.Enter(cancellationToken))
        {
            _cancellationPipe.PollWithCancellation(Handle, cancellationToken);

            nint ret;

            // Note that this call may get us suspended by way of a SIGTTIN signal if we are a background process and
            // the handle refers to a terminal.
            while ((ret = read(Handle, buffer, (nuint)buffer.Length)) == -1 &&
                Marshal.GetLastPInvokeError() == EINTR)
            {
                // Retry in case we get interrupted by a signal.
            }

            if (ret != -1)
                return (int)ret;

            var err = Marshal.GetLastPInvokeError();

            // EPIPE means the descriptor was probably redirected to a program that ended.
            return err == EPIPE
                ? 0
                : throw new TerminalException($"Could not read from {Name}: {new Win32Exception(err).Message}");
        }
    }
}
