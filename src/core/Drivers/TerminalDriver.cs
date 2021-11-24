namespace System.Drivers;

abstract partial class TerminalDriver
{
    public event Action<TerminalSize>? Resize
    {
        add
        {
            lock (_lock)
            {
                _resize += value;

                if (_resize != null)
                    ToggleResizeEvent(true);
            }
        }
        remove
        {
            lock (_lock)
            {
                _resize -= value;

                if (_resize == null)
                    ToggleResizeEvent(false);
            }
        }
    }

    public event Action<TerminalSignalContext>? Signal;

    public abstract TerminalReader StdIn { get; }

    public abstract TerminalWriter StdOut { get; }

    public abstract TerminalWriter StdError { get; }

    public bool IsRawMode { get; private set; }

    public TerminalSize Size
    {
        get
        {
            if (GetSize() is TerminalSize s)
                _size = s;

            return _size ?? throw new TerminalException("There is no terminal attached.");
        }
    }

    readonly object _lock = new();

    [SuppressMessage("Style", "IDE0052")]
    readonly PosixSignalRegistration _sigInt;

    [SuppressMessage("Style", "IDE0052")]
    readonly PosixSignalRegistration _sigQuit;

    TerminalSize? _size;

    Action<TerminalSize>? _resize;

    TerminalSize? _lastResize;

    [SuppressMessage("Reliability", "CA2000")]
    [SuppressMessage("ApiDesign", "RS0030")]
    protected TerminalDriver()
    {
        // Accessing this property has no particularly important effect on Windows, but it does do something important
        // on Unix: If a terminal is attached, it will force Console to initialize its System.Native portions, which
        // includes terminal settings. Thus, by doing this here, we ensure that if a user accidentally accesses Console
        // at some point later, it will not overwrite our terminal settings.
        _ = Console.In;

        // Try to prevent Console/Terminal intermixing from breaking stuff. This should prevent basic read/write calls
        // on Console from calling into internal classes like ConsolePal and StdInReader (which in turn call
        // System.Native functions that, among other things, change terminal settings).
        //
        // There are still many problematic properties and methods beyond these, but there is  not much we can do about
        // those.
        Console.SetIn(new InvalidTextReader());
        Console.SetOut(new InvalidTextWriter());
        Console.SetError(new InvalidTextWriter());

        void HandleSignal(PosixSignalContext context)
        {
            var args = new TerminalSignalContext(
                context.Signal switch
                {
                    PosixSignal.SIGHUP => TerminalSignal.Close,
                    PosixSignal.SIGINT => TerminalSignal.Interrupt,
                    PosixSignal.SIGQUIT => TerminalSignal.Quit,
                    PosixSignal.SIGTERM => TerminalSignal.Terminate,
                    _ => throw new TerminalException($"Received unexpected signal: {context.Signal}"),
                });

            Signal?.Invoke(args);

            context.Cancel = args.Cancel;
        }

        // Keep the registrations alive by storing them in fields.
        _sigInt = PosixSignalRegistration.Create(PosixSignal.SIGINT, HandleSignal);
        _sigQuit = PosixSignalRegistration.Create(PosixSignal.SIGQUIT, HandleSignal);
    }

    protected abstract TerminalSize? GetSize();

    protected void RefreshSize()
    {
        if (GetSize() is TerminalSize size)
        {
            _size = size;

            if (size != _lastResize)
            {
                _lastResize = size;

                // Do this on the thread pool to avoid breaking driver internals if an event handler misbehaves. This
                // event is also relatively low priority, so we do not care too much if the thread pool takes a bit of
                // time to get around to it.
                _ = ThreadPool.UnsafeQueueUserWorkItem(state => _resize?.Invoke(size), null);
            }
        }
    }

    protected virtual void ToggleResizeEvent(bool enable)
    {
    }

    public abstract void GenerateSignal(TerminalSignal signal);

    protected abstract void SetRawMode(bool raw);

    public void EnableRawMode()
    {
        lock (_lock)
            SetRawMode(IsRawMode = true);
    }

    public void DisableRawMode()
    {
        lock (_lock)
            SetRawMode(IsRawMode = false);
    }
}
