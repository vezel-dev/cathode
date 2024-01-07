using Vezel.Cathode.Native;

namespace Vezel.Cathode.Terminals;

internal sealed class NativeTerminalReader : TerminalReader
{
    // Note that the buffer size used affects how many characters the Windows console host will allow the user to type
    // in a single line (in cooked mode).
    private const int ReadBufferSize = 4096;

    public NativeVirtualTerminal Terminal { get; }

    public nuint Handle { get; }

    public override sealed Stream Stream { get; }

    public override sealed TextReader TextReader { get; }

    public override sealed bool IsValid { get; }

    public override sealed bool IsInteractive { get; }

    private readonly SemaphoreSlim _semaphore;

    private readonly Action<nuint, CancellationToken>? _cancellationHook;

    public NativeTerminalReader(
        NativeVirtualTerminal terminal,
        nuint handle,
        SemaphoreSlim semaphore,
        Action<nuint, CancellationToken>? cancellationHook)
    {
        Terminal = terminal;
        Handle = handle;
        _semaphore = semaphore;
        _cancellationHook = cancellationHook;
        Stream = new SynchronizedStream(new TerminalInputStream(this));
        TextReader =
            new SynchronizedTextReader(
                new StreamReader(Stream, Cathode.Terminal.Encoding, false, ReadBufferSize, true));
        IsValid = TerminalInterop.IsValid(handle, write: false);
        IsInteractive = TerminalInterop.IsInteractive(handle);
    }

    private unsafe int ReadPartialNative(scoped Span<byte> buffer, CancellationToken cancellationToken)
    {
        using var guard = Terminal.Control.Guard();

        // If the descriptor is invalid, just present the illusion to the user that it has been redirected to /dev/null
        // or something along those lines, i.e. return EOF.
        if (buffer is [] || !IsValid)
            return 0;

        using (_semaphore.Enter(cancellationToken))
        {
            _cancellationHook?.Invoke(Handle, cancellationToken);

            int progress;

            fixed (byte* p = buffer)
                TerminalInterop.Read(Handle, p, buffer.Length, &progress).ThrowIfError();

            return progress;
        }
    }

    protected override int ReadPartialCore(scoped Span<byte> buffer)
    {
        return ReadPartialNative(buffer, default);
    }

    protected override ValueTask<int> ReadPartialCoreAsync(Memory<byte> buffer, CancellationToken cancellationToken)
    {
        // We currently have no native async support.
        return cancellationToken.IsCancellationRequested
            ? ValueTask.FromCanceled<int>(cancellationToken)
            : new(Task.Run(() => ReadPartialNative(buffer.Span, cancellationToken), cancellationToken));
    }
}
