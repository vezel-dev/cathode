using Vezel.Cathode.Native;

namespace Vezel.Cathode.Terminals;

[SuppressMessage("", "CA1001")]
internal sealed class UnixCancellationPipe
{
    // TODO: Move this logic to src/native/driver-unix.c.

    private readonly AnonymousPipeServerStream _server;

    private readonly AnonymousPipeClientStream _client;

    private readonly bool _write;

    public UnixCancellationPipe(bool write)
    {
        _server = new(PipeDirection.Out);
        _client = new(PipeDirection.In, _server.ClientSafePipeHandle);
        _write = write;
    }

    public unsafe void PollWithCancellation(int fd, CancellationToken cancellationToken)
    {
        // Note that the runtime sets up a SIGPIPE handler for us.

        var unused = false;
        var pipeHandle = _client.SafePipeHandle;

        pipeHandle.DangerousAddRef(ref unused);

        try
        {
            var handles = stackalloc[]
            {
                (int)pipeHandle.DangerousGetHandle(),
                fd,
            };
            var results = stackalloc bool[2];

            using (var registration = cancellationToken.UnsafeRegister(
                static @this => Unsafe.As<UnixCancellationPipe>(@this!)._server.WriteByte(42), this))
                TerminalInterop.Poll(_write, handles, results, count: 2);

            // Were we canceled?
            if (results[0])
            {
                // Read the dummy byte that was written to indicate cancellation.
                _ = _client.ReadByte();

                throw new OperationCanceledException(cancellationToken);
            }
        }
        finally
        {
            pipeHandle.DangerousRelease();
        }
    }
}
