using Vezel.Cathode.Native;

namespace Vezel.Cathode.Terminals.Unix;

internal sealed class UnixTerminalReader : NativeTerminalReader
{
    private readonly UnixCancellationPipe _cancellationPipe = new(write: false);

    private readonly SemaphoreSlim _semaphore;

    public UnixTerminalReader(UnixVirtualTerminal terminal, nuint handle, SemaphoreSlim semaphore)
        : base(terminal, handle)
    {
        _semaphore = semaphore;
    }

    protected override unsafe int ReadPartialNative(scoped Span<byte> buffer, CancellationToken cancellationToken)
    {
        using var guard = Terminal.Control.Guard();

        // If the descriptor is invalid, just present the illusion to the user that it has been redirected to /dev/null
        // or something along those lines, i.e. return EOF.
        if (buffer is [] || !IsValid)
            return 0;

        using (_semaphore.Enter(cancellationToken))
        {
            _cancellationPipe.PollWithCancellation(Handle, cancellationToken);

            int progress;

            fixed (byte* p = buffer)
                TerminalInterop.Read(Handle, p, buffer.Length, &progress).ThrowIfError();

            return progress;
        }
    }
}
