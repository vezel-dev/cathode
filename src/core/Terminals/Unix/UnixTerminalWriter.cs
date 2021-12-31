using static System.Unix.UnixPInvoke;

namespace System.Terminals.Unix;

sealed class UnixTerminalWriter : NativeTerminalWriter<UnixVirtualTerminal, int>
{
    readonly object _lock;

    public UnixTerminalWriter(UnixVirtualTerminal terminal, string name, int handle, object @lock)
        : base(terminal, name, handle)
    {
        _lock = @lock;
    }

    protected override unsafe int WriteBufferCore(ReadOnlySpan<byte> buffer, CancellationToken cancellationToken)
    {
        using var guard = Terminal.Control.Guard();

        // If the descriptor is invalid, just present the illusion to the user that it has been redirected to /dev/null
        // or something along those lines, i.e. pretend we wrote everything.
        if (buffer.IsEmpty || !IsValid)
            return buffer.Length;

        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            fixed (byte* p = &MemoryMarshal.GetReference(buffer))
            {
                while (true)
                {
                    nint ret;

                    // Note that this call may get us suspended by way of a SIGTTOU signal if we are a background
                    // process, the handle refers to a terminal, and the TOSTOP bit is set (we disable TOSTOP but there
                    // are ways that it could get set anyway).
                    while ((ret = write(Handle, p, (nuint)buffer.Length)) == -1 &&
                        Marshal.GetLastPInvokeError() == EINTR)
                    {
                    }

                    if (ret != -1)
                        return (int)ret;

                    var err = Marshal.GetLastPInvokeError();

                    // EPIPE means the descriptor was probably redirected to a program that ended.
                    if (err == EPIPE)
                        return 0;

                    // The file descriptor might have been configured as non-blocking. Instead of busily trying to write
                    // over and over, poll until we can write and then try again.
                    if (Terminal.PollHandles(err, POLLOUT, stackalloc[] { Handle }))
                        continue;

                    throw new TerminalException($"Could not write to {Name}: {new Win32Exception(err).Message}");
                }
            }
        }
    }
}