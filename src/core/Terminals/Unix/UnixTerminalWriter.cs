using Vezel.Cathode.Native;

namespace Vezel.Cathode.Terminals.Unix;

internal sealed class UnixTerminalWriter : NativeTerminalWriter
{
    private readonly SemaphoreSlim _semaphore;

    public UnixTerminalWriter(UnixVirtualTerminal terminal, nuint handle, SemaphoreSlim semaphore)
        : base(terminal, handle)
    {
        _semaphore = semaphore;
    }

    protected override unsafe int WritePartialNative(scoped ReadOnlySpan<byte> buffer, CancellationToken cancellationToken)
    {
        using var guard = Terminal.Control.Guard();

        // If the descriptor is invalid, just present the illusion to the user that it has been redirected to /dev/null
        // or something along those lines, i.e. pretend we wrote everything.
        if (buffer is [] || !IsValid)
            return buffer.Length;

        using (_semaphore.Enter(cancellationToken))
        {
            int progress;

            fixed (byte* p = buffer)
                TerminalInterop.Write(Handle, p, buffer.Length, &progress).ThrowIfError();

            return progress;
        }
    }
}
