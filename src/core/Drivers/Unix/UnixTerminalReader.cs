using static System.Unix.UnixPInvoke;

namespace System.Drivers.Unix;

sealed class UnixTerminalReader : DriverTerminalReader<UnixTerminalDriver, int>
{
    readonly object _lock;

    readonly UnixCancellationPipe _cancellationPipe;

    public UnixTerminalReader(
        UnixTerminalDriver driver,
        string name,
        int handle,
        UnixCancellationPipe cancellationPipe,
        object @lock)
        : base(driver, name, handle)
    {
        _lock = @lock;
        _cancellationPipe = cancellationPipe;
    }

    protected override unsafe void ReadCore(Span<byte> data, out int count, CancellationToken cancellationToken)
    {
        // If the descriptor is invalid, just present the illusion to the user that it has been redirected to /dev/null
        // or something along those lines, i.e. return EOF.
        if (data.IsEmpty || !IsValid)
        {
            count = 0;

            return;
        }

        lock (_lock)
        {
            try
            {
                _cancellationPipe.PollWithCancellation(Handle, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                count = 0;

                throw;
            }

            fixed (byte* p = data)
            {
                nint ret;

                // Note that this call may get us suspended by way of a SIGTTIN signal if we are a background process
                // and the handle refers to a terminal.
                while ((ret = read(Handle, p, (nuint)data.Length)) == -1 &&
                    Marshal.GetLastPInvokeError() == EINTR)
                {
                    // Retry in case we get interrupted by a signal.
                }

                if (ret != -1)
                {
                    count = (int)ret;

                    return;
                }

                var err = Marshal.GetLastPInvokeError();

                // The descriptor was probably redirected to a program that ended. Just silently ignore this
                // situation.
                //
                // The strange condition where errno is zero happens e.g. on Linux if the process is killed while
                // blocking in the read system call.
                if (err is 0 or EPIPE)
                {
                    count = 0;

                    return;
                }

                throw new TerminalException($"Could not read from {Name}: {new Win32Exception(err).Message}");
            }
        }
    }
}
