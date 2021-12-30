namespace System;

public abstract class SystemVirtualTerminal : VirtualTerminal
{
    public override event Action<TerminalSize>? Resized
    {
        add
        {
            lock (_sizeLock)
            {
                _resize += value;

                if (_resize != null)
                    _resizeEvent.Set();
            }
        }
        remove
        {
            lock (_sizeLock)
            {
                _resize -= value;

                if (_resize == null)
                    _resizeEvent.Reset();
            }
        }
    }

    public override event Action<TerminalSignalContext>? Signaled;

    public TerminalControl Control { get; } = new();

    public override bool IsRawMode => _rawMode;

    [SuppressMessage("Design", "CA1065")]
    public override TerminalSize Size
    {
        get
        {
            if (QuerySize() is TerminalSize s)
                _size = s;

            return _size ?? throw new TerminalNotAttachedException();
        }
    }

    public TimeSpan SizePollingInterval
    {
        get => _sizeInterval;
        set
        {
            using var guard = Control.Guard();

            _ = value >= TimeSpan.Zero ? true : throw new ArgumentOutOfRangeException(nameof(value));

            _sizeInterval = value;
        }
    }

    readonly object _sizeLock = new();

    readonly object _rawLock = new();

    readonly ManualResetEventSlim _resizeEvent = new();

    readonly HashSet<TerminalProcess> _processes = new();

    [SuppressMessage("Style", "IDE0052")]
    readonly PosixSignalRegistration _sigHup;

    [SuppressMessage("Style", "IDE0052")]
    readonly PosixSignalRegistration _sigInt;

    [SuppressMessage("Style", "IDE0052")]
    readonly PosixSignalRegistration _sigQuit;

    [SuppressMessage("Style", "IDE0052")]
    readonly PosixSignalRegistration _sigTerm;

    bool _rawMode;

    TerminalSize? _size;

    TimeSpan _sizeInterval = TimeSpan.FromMilliseconds(100);

    Action<TerminalSize>? _resize;

    private protected SystemVirtualTerminal()
    {
        var thread = new Thread(() =>
        {
            while (true)
            {
                _resizeEvent.Wait();

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
                    _ => throw new NotSupportedException($"Received unexpected signal: {context.Signal}"),
                });

            Signaled?.Invoke(ctx);

            context.Cancel = ctx.Cancel;

            // The following is a workaround: see https://github.com/alexrp/system-terminal/issues/64
            // When the debugger is attached and we are on Windows with an active ReadConsole
            // the threads will be stuck and CTRL+C won't exit the process.
            if (Debugger.IsAttached && OperatingSystem.IsWindows() && !ctx.Cancel && context.Signal == PosixSignal.SIGINT)
            {
                // Emit the same exit code as CTRL+C (0x_C000_013A)
                Environment.Exit(-1073741510);
            }
        }

        // Keep the registrations alive by storing them in fields.
        _sigHup = PosixSignalRegistration.Create(PosixSignal.SIGHUP, HandleSignal);
        _sigInt = PosixSignalRegistration.Create(PosixSignal.SIGINT, HandleSignal);
        _sigQuit = PosixSignalRegistration.Create(PosixSignal.SIGQUIT, HandleSignal);
        _sigTerm = PosixSignalRegistration.Create(PosixSignal.SIGTERM, HandleSignal);
    }

    protected abstract TerminalSize? QuerySize();

    protected void RefreshSize()
    {
        if (QuerySize() is not TerminalSize size)
        {
            // These environment variables are usually set by shells. Use them as a fallback.
            if (!int.TryParse(Environment.GetEnvironmentVariable("COLUMNS"), out var columns) ||
                !int.TryParse(Environment.GetEnvironmentVariable("LINES"), out var lines))
                return;

            size = new(columns, lines);
        }

        // Serialize this bit since the Unix implementation can call this method from its SIGWINCH/SIGCONT signal
        // handler. We do not want to raise the Resize event twice for the same size value.
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

        // Do this on the thread pool to avoid breaking internals if an event handler misbehaves.
        _ = ThreadPool.UnsafeQueueUserWorkItem(state => _resize?.Invoke(size), null);
    }

    protected abstract void SendSignal(int pid, TerminalSignal signal);

    public override void GenerateSignal(TerminalSignal signal)
    {
        using var guard = Control.Guard();

        SendSignal(0, signal);
    }

    internal void SignalProcess(TerminalProcess process, TerminalSignal signal)
    {
        SendSignal(process.Id, signal);
    }

    protected abstract void SetRawMode(bool raw);

    public override void EnableRawMode()
    {
        using var guard = Control.Guard();

        lock (_rawLock)
        {
            if (_processes.Count != 0)
                throw new InvalidOperationException(
                    "Cannot enable raw mode with non-redirected child processes running.");

            SetRawMode(true);

            _rawMode = true;
        }
    }

    public override void DisableRawMode()
    {
        using var guard = Control.Guard();

        lock (_rawLock)
        {
            if (_processes.Count != 0)
                throw new InvalidOperationException(
                    "Cannot disable raw mode with non-redirected child processes running.");

            SetRawMode(false);

            _rawMode = false;
        }
    }

    internal void StartProcess(Func<TerminalProcess> starter)
    {
        lock (_rawLock)
        {
            // The vast majority of programs expect to start in cooked mode. Enforce that we are in cooked mode while
            // any child processes that could be using the terminal are running.
            if (_rawMode)
                throw new InvalidOperationException("Cannot start non-redirected child processes in raw mode.");

            // Guard here since this locks us into cooked mode until all non-redirected processes are gone.
            using var guard = Control.Guard();

            _ = _processes.Add(starter());
        }
    }

    internal void ReapProcess(TerminalProcess process)
    {
        lock (_rawLock)
        {
            _ = _processes.Remove(process);

            if (_processes.Count != 0)
                return;

            try
            {
                // Child processes may have messed up the terminal settings. Restore them just in case.
                SetRawMode(false);
            }
            catch (Exception e) when (e is TerminalNotAttachedException or TerminalConfigurationException)
            {
            }
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract void DangerousRestoreSettings();
}
