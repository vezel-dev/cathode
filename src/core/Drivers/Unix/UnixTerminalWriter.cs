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

    protected override unsafe void WriteCore(ReadOnlySpan<byte> data, out int count)
    {
        // If the descriptor is invalid, just present the illusion to the user that it has been redirected to /dev/null
        // or something along those lines, i.e. pretend we wrote everything.
        if (data.IsEmpty || !IsValid)
        {
            count = data.Length;

            return;
        }

        lock (_lock)
        {
            fixed (byte* p = data)
            {
                count = 0;

                while (count < data.Length)
                {
                    long ret;

                    while ((ret = write(Handle, p + count, (nuint)(data.Length - count))) == -1 &&
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

                    // The descriptor was probably redirected to a program that ended. Just silently ignore this
                    // situation.
                    if (err == EPIPE)
                        break;

                    // The file descriptor has been configured as non-blocking. Instead of busily trying to write over
                    // and over, poll until we can write and then try again.
                    if (Driver.PollHandle(err, Handle, POLLOUT))
                        continue;

                    throw new TerminalException($"Could not write to {Name}: {new Win32Exception(err).Message}");
                }
            }
        }
    }
}
