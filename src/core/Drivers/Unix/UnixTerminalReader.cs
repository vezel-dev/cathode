using static System.Unix.UnixPInvoke;

namespace System.Drivers.Unix;

sealed class UnixTerminalReader : DriverTerminalReader<UnixTerminalDriver, int>
{
    readonly object _lock;

    public UnixTerminalReader(UnixTerminalDriver driver, string name, int handle, object @lock)
        : base(driver, name, handle)
    {
        _lock = @lock;
    }

    protected override unsafe void ReadCore(Span<byte> data, out int count)
    {
        // If the descriptor is invalid, just present the illusion to the user that it has been redirected to /dev/null
        // or something along those lines, i.e. return EOF.
        if (data.IsEmpty || !IsValid)
        {
            count = 0;

            return;
        }

        long ret;

        lock (_lock)
        {
            while (true)
            {
                fixed (byte* p = data)
                {
                    // Note that this call may get us suspended by way of a SIGTTIN signal if we are a background
                    // process and the handle refers to a terminal.
                    while ((ret = read(Handle, p, (nuint)data.Length)) == -1 &&
                        Marshal.GetLastPInvokeError() == EINTR)
                    {
                        // Retry in case we get interrupted by a signal.
                    }

                    if (ret != -1)
                        break;

                    var err = Marshal.GetLastPInvokeError();

                    // The descriptor was probably redirected to a program that ended. Just silently ignore this
                    // situation.
                    //
                    // The strange condition where errno is zero happens e.g. on Linux if the process is killed while
                    // blocking in the read system call.
                    if (err is 0 or EPIPE)
                    {
                        ret = 0;

                        break;
                    }

                    // The file descriptor has been configured as non-blocking. Instead of busily trying to read over
                    // and over, poll until we can write and then try again.
                    if (Driver.PollHandle(err, Handle, POLLIN))
                        continue;

                    throw new TerminalException($"Could not read from {Name}: {new Win32Exception(err).Message}");
                }
            }
        }

        count = (int)ret;
    }
}
