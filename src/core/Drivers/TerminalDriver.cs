namespace System.Drivers;

abstract class TerminalDriver
{
    public event Action<TerminalSize>? Resize
    {
        add
        {
            lock (_sizeLock)
            {
                _resize += value;

                if (_resize != null)
                    _event.Set();
            }
        }
        remove
        {
            lock (_sizeLock)
            {
                _resize -= value;

                if (_resize == null)
                    _event.Reset();
            }
        }
    }

    public event Action<TerminalSignalContext>? Signal;

    public abstract TerminalReader StandardIn { get; }

    public abstract TerminalWriter StandardOut { get; }

    public abstract TerminalWriter StandardError { get; }

    public abstract TerminalReader TerminalIn { get; }

    public abstract TerminalWriter TerminalOut { get; }

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

    public TimeSpan SizePollingInterval
    {
        get => _sizeInterval;
        set
        {
            _ = value >= TimeSpan.Zero ? true : throw new ArgumentOutOfRangeException(nameof(value));

            _sizeInterval = value;
        }
    }

    readonly object _sizeLock = new();

    readonly object _rawLock = new();

    readonly ManualResetEventSlim _event = new();

    [SuppressMessage("Style", "IDE0052")]
    readonly PosixSignalRegistration _sigHup;

    [SuppressMessage("Style", "IDE0052")]
    readonly PosixSignalRegistration _sigInt;

    [SuppressMessage("Style", "IDE0052")]
    readonly PosixSignalRegistration _sigQuit;

    [SuppressMessage("Style", "IDE0052")]
    readonly PosixSignalRegistration _sigTerm;

    TerminalSize? _size;

    TimeSpan _sizeInterval = TimeSpan.FromMilliseconds(100);

    Action<TerminalSize>? _resize;

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

        var thread = new Thread(() =>
        {
            while (true)
            {
                _event.Wait();

                RefreshSize();

                Thread.Sleep(_sizeInterval);
            }
        })
        {
            Name = "Terminal Resize Poller",
            IsBackground = true,
        };

        thread.Start();

        void HandleSignal(PosixSignalContext context)
        {
            var ctx = new TerminalSignalContext(
                context.Signal switch
                {
                    PosixSignal.SIGHUP => TerminalSignal.Close,
                    PosixSignal.SIGINT => TerminalSignal.Interrupt,
                    PosixSignal.SIGQUIT => TerminalSignal.Quit,
                    PosixSignal.SIGTERM => TerminalSignal.Terminate,
                    _ => throw new TerminalException($"Received unexpected signal: {context.Signal}"),
                });

            Signal?.Invoke(ctx);

            context.Cancel = ctx.Cancel;
        }

        // Keep the registrations alive by storing them in fields.
        _sigHup = PosixSignalRegistration.Create(PosixSignal.SIGHUP, HandleSignal);
        _sigInt = PosixSignalRegistration.Create(PosixSignal.SIGINT, HandleSignal);
        _sigQuit = PosixSignalRegistration.Create(PosixSignal.SIGQUIT, HandleSignal);
        _sigTerm = PosixSignalRegistration.Create(PosixSignal.SIGTERM, HandleSignal);
    }

    protected abstract TerminalSize? GetSize();

    protected void RefreshSize()
    {
        if (GetSize() is not TerminalSize size)
        {
            // These environment variables are usually set by shells. Use them as a fallback.
            if (!int.TryParse(Environment.GetEnvironmentVariable("COLUMNS"), out var columns) ||
                !int.TryParse(Environment.GetEnvironmentVariable("LINES"), out var lines))
                return;

            size = new(columns, lines);
        }

        // Serialize this bit since the Unix driver can call this method from its SIGWINCH/SIGCONT signal handler. We do
        // not want to raise the Resize event twice for the same size value.
        lock (_sizeLock)
        {
            var old = _size;

            _size = size;

            // We do not want to raise the event if the size has not been set before. This just indicates that the user
            // is either retrieving the Size property for the first time or is installing an event handler without
            // having accessed the Size property yet.
            if (old == null || size == old)
                return;
        }

        // Do this on the thread pool to avoid breaking driver internals if an event handler misbehaves. This event is
        // also relatively low priority, so we do not care too much if the thread pool takes a bit of time to get around
        // to it.
        _ = ThreadPool.UnsafeQueueUserWorkItem(state => _resize?.Invoke(size), null);
    }

    public abstract void GenerateSignal(TerminalSignal signal);

    protected abstract void SetRawMode(bool raw);

    public void EnableRawMode()
    {
        lock (_rawLock)
            SetRawMode(IsRawMode = true);
    }

    public void DisableRawMode()
    {
        lock (_rawLock)
            SetRawMode(IsRawMode = false);
    }

    public abstract void RestoreSettings();
}

abstract class TerminalDriver<THandle> : TerminalDriver
{
    public abstract bool IsHandleValid(THandle handle, bool write);

    public abstract bool IsHandleInteractive(THandle handle);
}
