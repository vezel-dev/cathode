using static System.Unix.UnixConstants;
using static System.Unix.UnixPInvoke;

namespace System.Drivers.Unix;

sealed class UnixTerminalReader : DefaultTerminalReader
{
    public int Handle { get; }

    public override bool IsRedirected => UnixTerminalUtility.IsRedirected(Handle);

    readonly object _lock = new();

    readonly UnixTerminalDriver _driver;

    public UnixTerminalReader(UnixTerminalDriver driver, int handle)
    {
        Handle = handle;
        _driver = driver;
    }

    public override unsafe int Read(Span<byte> data)
    {
        if (data.IsEmpty)
            return 0;

        long ret;

        lock (_lock)
        {
            while (true)
            {
                fixed (byte* p = data)
                {
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
                    // The strange condition where errno is zero happens e.g. on Linux if the process is killed
                    // while blocking in the read system call.
                    if (err is 0 or EPIPE)
                    {
                        ret = 0;

                        break;
                    }

                    // The file descriptor has been configured as non-blocking. Instead of busily trying to read
                    // over and over, poll until we can write and then try again.
                    if (_driver.PollHandle(err, Handle, POLLIN))
                        continue;

                    throw new TerminalException($"Could not read from standard input: {err}");
                }
            }
        }

        return (int)ret;
    }
}
