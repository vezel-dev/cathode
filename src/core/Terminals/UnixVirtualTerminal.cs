namespace Vezel.Cathode.Terminals;

internal sealed class UnixVirtualTerminal : NativeVirtualTerminal
{
    public override event Action? Resumed;

    public static UnixVirtualTerminal Instance { get; } = new();

    private readonly PosixSignalRegistration _sigWinch;

    private readonly PosixSignalRegistration _sigCont;

    private readonly PosixSignalRegistration _sigChld;

    public UnixVirtualTerminal()
    {
        void HandleSignal(PosixSignalContext context)
        {
            // If we are being restored from the background (SIGCONT), it is possible and likely that terminal settings
            // have been mangled, so restore them.
            //
            // This is a best-effort thing. The reality is that, since this signal handler method gets called in a
            // thread after the process has fully woken up, other code may already be trying to interact with the
            // terminal again. There is nothing we can really do about this race condition.
            if (context.Signal == PosixSignal.SIGCONT)
            {
                try
                {
                    ChangeRawMode(IsRawMode, flush: false, check: null);
                }
                catch (Exception e) when (e is TerminalNotAttachedException or TerminalConfigurationException)
                {
                    // Either there was no terminal attached to begin with, or it has disappeared since we were stopped.
                    // In either case, the program can no longer read from or write to the terminal, so terminal
                    // settings are irrelevant.
                }

                // Do this on the thread pool to avoid breaking internals if an event handler misbehaves.
                _ = ThreadPool.UnsafeQueueUserWorkItem(
                    static @this => @this.Resumed?.Invoke(), this, preferLocal: true);
            }

            // Terminal width/height will definitely have changed for SIGWINCH, and might have changed for SIGCONT and
            // SIGCHLD. On Unix systems, signals let us respond much more quickly to a change in terminal size.
            RefreshSize();

            // Prevent System.Native from overwriting our terminal settings.
            context.Cancel = true;
        }

        // Keep the registrations alive by storing them in fields.
        _sigWinch = PosixSignalRegistration.Create(PosixSignal.SIGWINCH, HandleSignal);
        _sigCont = PosixSignalRegistration.Create(PosixSignal.SIGCONT, HandleSignal);
        _sigChld = PosixSignalRegistration.Create(PosixSignal.SIGCHLD, HandleSignal);
    }

    protected override unsafe Action<nuint, CancellationToken> CreateCancellationHook(bool write)
    {
        var pipe = new UnixCancellationPipe(write);

        return (descriptor, cancellationToken) =>
        {
            if (cancellationToken.CanBeCanceled)
                pipe.PollWithCancellation(*(int*)descriptor, cancellationToken);
        };
    }
}
