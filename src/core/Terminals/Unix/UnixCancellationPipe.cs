using static System.Unix.UnixPInvoke;

namespace System.Terminals.Unix;

sealed class UnixCancellationPipe
{
    readonly UnixVirtualTerminal _terminal;

    readonly int _readHandle;

    readonly int _writeHandle;

    public UnixCancellationPipe(UnixVirtualTerminal terminal)
    {
        _terminal = terminal;
        (_readHandle, _writeHandle) = terminal.CreatePipePair();
    }

    public unsafe void PollWithCancellation(int handle, CancellationToken cancellationToken)
    {
        // Note that the runtime sets up a SIGPIPE handler for us.

        static void CancellationCallback(object? state)
        {
            byte dummy = 42;
            nint ret;

            // We should get no errors other than EINTR from writing to the pipe.
            while ((ret = write(((UnixCancellationPipe)state!)._writeHandle, &dummy, 1)) == -1 &&
                Marshal.GetLastPInvokeError() == EINTR)
            {
                // Retry in case we get interrupted by a signal.
            }
        }

        Span<int> handles = stackalloc int[]
        {
            _readHandle,
            handle,
        };

        using (var registration = cancellationToken.UnsafeRegister(CancellationCallback, this))
            if (!_terminal.PollHandles(null, POLLIN, handles))
                return;

        // Were we canceled?
        if ((handles[0] & POLLIN) != 0)
        {
            byte dummy;

            // We should get no errors other than EINTR from reading from the pipe.
            while (read(_readHandle, &dummy, 1) == -1 &&
                Marshal.GetLastPInvokeError() == EINTR)
            {
                // Retry in case we get interrupted by a signal.
            }

            throw new OperationCanceledException(cancellationToken);
        }
    }
}
