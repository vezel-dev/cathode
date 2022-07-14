using static Vezel.Cathode.Unix.UnixPInvoke;

namespace Vezel.Cathode.Terminals.Unix;

sealed class UnixCancellationPipe
{
    readonly UnixVirtualTerminal _terminal;

    readonly AnonymousPipeServerStream _server;

    readonly AnonymousPipeClientStream _client;

    public UnixCancellationPipe(UnixVirtualTerminal terminal)
    {
        _terminal = terminal;
        _server = new(PipeDirection.Out);
        _client = new(PipeDirection.In, _server.ClientSafePipeHandle);
    }

    public unsafe void PollWithCancellation(int handle, CancellationToken cancellationToken)
    {
        // Note that the runtime sets up a SIGPIPE handler for us.

        var handles = (stackalloc[] { (int)_client.SafePipeHandle.DangerousGetHandle(), handle });

        using (var registration = cancellationToken.UnsafeRegister(
            state => ((UnixCancellationPipe)state!)._server.WriteByte(42), this))
            if (!_terminal.PollHandles(null, POLLIN, handles))
                return;

        // Were we canceled?
        if ((handles[0] & POLLIN) != 0)
        {
            // Read the dummy byte that was written to indicate cancellation.
            _ = _client.ReadByte();

            throw new OperationCanceledException(cancellationToken);
        }
    }
}
