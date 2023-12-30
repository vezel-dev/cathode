using Vezel.Cathode.Native;

namespace Vezel.Cathode.Terminals.Unix;

[SuppressMessage("", "CA1001")]
internal sealed class UnixCancellationPipe
{
    private readonly UnixVirtualTerminal _terminal;

    private readonly AnonymousPipeServerStream _server;

    private readonly AnonymousPipeClientStream _client;

    public UnixCancellationPipe(UnixVirtualTerminal terminal)
    {
        _terminal = terminal;
        _server = new(PipeDirection.Out);
        _client = new(PipeDirection.In, _server.ClientSafePipeHandle);
    }

    public unsafe void PollWithCancellation(nuint handle, CancellationToken cancellationToken)
    {
        // Note that the runtime sets up a SIGPIPE handler for us.

        var unused = false;
        var pipeHandle = _client.SafePipeHandle;

        pipeHandle.DangerousAddRef(ref unused);

        try
        {
            var handles = stackalloc[]
            {
                (nuint)pipeHandle.DangerousGetHandle(),
                handle,
            };
            var results = stackalloc bool[2];

            using (var registration = cancellationToken.UnsafeRegister(
                static @this => Unsafe.As<UnixCancellationPipe>(@this!)._server.WriteByte(42), this))
                TerminalInterop.Poll(write: false, handles, results, count: 2);

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
