using static System.Unix.UnixConstants;
using static System.Unix.UnixPInvoke;

namespace System.Drivers.Unix;

sealed class UnixTerminalWriter : DefaultTerminalWriter
{
    public int Handle { get; }

    public override bool IsRedirected => UnixTerminalUtility.IsRedirected(Handle);

    readonly object _lock;

    readonly UnixTerminalDriver _driver;

    public UnixTerminalWriter(string name, int handle, object @lock, UnixTerminalDriver driver)
        : base(name)
    {
        Handle = handle;
        _lock = @lock;
        _driver = driver;
    }

    protected override unsafe void WriteCore(ReadOnlySpan<byte> data, out int count)
    {
        if (data.IsEmpty)
        {
            count = 0;

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
                    if (_driver.PollHandle(err, Handle, POLLOUT))
                        continue;

                    throw new TerminalException($"Could not write to {Name}: {new Win32Exception(err).Message}");
                }
            }
        }
    }
}
