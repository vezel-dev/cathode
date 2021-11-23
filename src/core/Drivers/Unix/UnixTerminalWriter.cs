using static System.Unix.UnixConstants;
using static System.Unix.UnixPInvoke;

namespace System.Drivers.Unix;

sealed class UnixTerminalWriter : DefaultTerminalWriter
{
    public int Handle { get; }

    public override bool IsRedirected => UnixTerminalUtility.IsRedirected(Handle);

    readonly object _lock = new();

    readonly UnixTerminalDriver _driver;

    readonly string _name;

    public UnixTerminalWriter(UnixTerminalDriver driver, int handle, string name)
    {
        Handle = handle;
        _driver = driver;
        _name = name;
    }

    public override unsafe void Write(ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty)
            return;

        lock (_lock)
        {
            var progress = 0;

            fixed (byte* p = data)
            {
                var len = data.Length;

                while (progress < len)
                {
                    long ret;

                    while ((ret = write(Handle, p + progress, (nuint)(len - progress))) == -1 &&
                        Marshal.GetLastPInvokeError() == EINTR)
                    {
                    }

                    // The descriptor has been closed by someone else. Just silently ignore this situation.
                    if (ret == 0)
                        break;

                    if (ret != -1)
                    {
                        progress += (int)ret;

                        continue;
                    }

                    var err = Marshal.GetLastPInvokeError();

                    // The descriptor was probably redirected to a program that ended. Just silently ignore this
                    // situation.
                    if (err == EPIPE)
                        break;

                    // The file descriptor has been configured as non-blocking. Instead of busily trying to write
                    // over and over, poll until we can write and then try again.
                    if (_driver.PollHandle(err, Handle, POLLOUT))
                        continue;

                    throw new TerminalException($"Could not write to standard {_name}: {err}");
                }
            }
        }
    }
}
