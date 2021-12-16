using static System.Unix.UnixPInvoke;

namespace System.Drivers.Unix;

sealed class UnixCancellationPipe
{
    readonly UnixTerminalDriver _driver;

    readonly int _readHandle;

    readonly int _writeHandle;

    public UnixCancellationPipe(UnixTerminalDriver driver)
    {
        _driver = driver;
        (_readHandle, _writeHandle) = driver.CreatePipePair();
    }

    public unsafe void PollWithCancellation(int handle, CancellationToken cancellationToken)
    {
        // Note that the runtime sets up a SIGPIPE handler for us.

        static void CancellationCallback(object? state)
        {
            ReadOnlySpan<byte> dummy = stackalloc byte[]
            {
                42,
            };

            fixed (byte* p = dummy)
            {
                nint ret;

                // We should get no errors other than EINTR from writing to the pipe.
                while ((ret = write(((UnixCancellationPipe)state!)._writeHandle, p, (nuint)dummy.Length)) == -1 &&
                    Marshal.GetLastPInvokeError() == EINTR)
                {
                    // Retry in case we get interrupted by a signal.
                }
            }
        }

        Span<int> handles = stackalloc int[]
        {
            _readHandle,
            handle,
        };

        using (var registration = cancellationToken.UnsafeRegister(CancellationCallback, this))
            if (!_driver.PollHandles(null, POLLIN, handles))
                return;

        // Were we canceled?
        if ((handles[0] & POLLIN) != 0)
        {
            Span<byte> dummy = stackalloc byte[1];

            fixed (byte* p = dummy)
            {
                // We should get no errors other than EINTR from reading from the pipe.
                while (read(_readHandle, p, (nuint)dummy.Length) == -1 &&
                    Marshal.GetLastPInvokeError() == EINTR)
                {
                    // Retry in case we get interrupted by a signal.
                }
            }

            throw new OperationCanceledException(cancellationToken);
        }
    }
}
