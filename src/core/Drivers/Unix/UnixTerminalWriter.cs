using static System.Unix.UnixPInvoke;

namespace System.Drivers.Unix;

sealed class UnixTerminalWriter : DriverTerminalWriter<UnixTerminalDriver, int>
{
    readonly object _lock;

    public UnixTerminalWriter(UnixTerminalDriver driver, string name, int handle, object @lock)
        : base(driver, name, handle)
    {
        _lock = @lock;
    }

    protected override unsafe int WriteBufferCore(ReadOnlySpan<byte> buffer, CancellationToken cancellationToken)
    {
        // If the descriptor is invalid, just present the illusion to the user that it has been redirected to /dev/null
        // or something along those lines, i.e. pretend we wrote everything.
        if (buffer.IsEmpty || !IsValid)
            return buffer.Length;

        cancellationToken.ThrowIfCancellationRequested();

        var count = 0;

        lock (_lock)
        {
            fixed (byte* p = &MemoryMarshal.GetReference(buffer))
            {
                while (count < buffer.Length)
                {
                    nint ret;

                    // Note that this call may get us suspended by way of a SIGTTOU signal if we are a background
                    // process, the handle refers to a terminal, and the TOSTOP bit is set (we disable TOSTOP in the
                    // drivers but there are ways that it could get set anyway).
                    while ((ret = write(Handle, p + count, (nuint)(buffer.Length - count))) == -1 &&
                        Marshal.GetLastPInvokeError() == EINTR)
                    {
                    }

                    // The descriptor has been closed by someone else. Just silently ignore this situation.
                    if (ret == 0)
                        break;

                    if (ret != -1)
                    {
                        count += (int)ret;

                        continue;
                    }

                    var err = Marshal.GetLastPInvokeError();

                    // EPIPE means the descriptor was probably redirected to a program that ended.
                    if (err == EPIPE)
                        break;

                    // The file descriptor might have been configured as non-blocking. Instead of busily trying to write
                    // over and over, poll until we can write and then try again.
                    if (Driver.PollHandles(err, POLLOUT, stackalloc[] { Handle }))
                        continue;

                    // At this point there was an actual I/O error. We only want to throw if we did not manage to write
                    // anything so far. If we did manage to write something, the error should happen again the next time
                    // we are called, but this time without us managing to write anything.
                    if (count != 0)
                        break;

                    throw new TerminalException($"Could not write to {Name}: {new Win32Exception(err).Message}");
                }
            }
        }

        return count;
    }
}
