using Vezel.Cathode.Processes;

namespace Vezel.Cathode;

[SuppressMessage("", "CA1001")]
public abstract class SystemVirtualTerminal : VirtualTerminal
{
    public override sealed event Action<Size>? Resized
    {
        add
        {
            lock (_sizeLock)
            {
                _resized += value;

                if (_resized != null)
                    _resizeEvent.Set();
            }
        }

        remove
        {
            lock (_sizeLock)
            {
                _resized -= value;

                if (_resized == null)
                    _resizeEvent.Reset();
            }
        }
    }

    public override sealed event Action<TerminalSignalContext>? Signaled
    {
        add
        {
            lock (_signalLock)
            {
                _signaled += value;

                if (_signaled == null)
                    return;

                [SuppressMessage("", "CA1065")]
                void HandleSignal(PosixSignalContext context)
                {
                    var ctx = new TerminalSignalContext(
                        context.Signal switch
                        {
                            PosixSignal.SIGHUP => TerminalSignal.Close,
                            PosixSignal.SIGINT => TerminalSignal.Interrupt,
                            PosixSignal.SIGQUIT => TerminalSignal.Quit,
                            PosixSignal.SIGTERM => TerminalSignal.Terminate,
                            _ => throw new UnreachableException(),
                        });

                    _signaled?.Invoke(ctx);

                    context.Cancel = ctx.Cancel;
                }

                _sigHup = PosixSignalRegistration.Create(PosixSignal.SIGHUP, HandleSignal);
                _sigInt = PosixSignalRegistration.Create(PosixSignal.SIGINT, HandleSignal);
                _sigQuit = PosixSignalRegistration.Create(PosixSignal.SIGQUIT, HandleSignal);
                _sigTerm = PosixSignalRegistration.Create(PosixSignal.SIGTERM, HandleSignal);
            }
        }

        remove
        {
            lock (_signalLock)
            {
                _signaled -= value;

                if (_signaled != null)
                    return;

                _sigHup!.Dispose();
                _sigInt!.Dispose();
                _sigQuit!.Dispose();
                _sigTerm!.Dispose();

                _sigHup = null;
                _sigInt = null;
                _sigQuit = null;
                _sigTerm = null;
            }
        }
    }

    public TerminalControl Control { get; } = new();

    public override sealed bool IsRawMode
    {
        get
        {
            lock (_rawLock)
                return GetMode();
        }
    }

    [SuppressMessage("", "CA1065")]
    public override sealed Size Size
    {
        get
        {
            if (QuerySize() is { } s)
                _size = s;

            return _size ?? throw new TerminalNotAttachedException();
        }
    }

    public TimeSpan SizePollingInterval
    {
        get => _sizeInterval;
        set
        {
            Check.Range(value >= TimeSpan.Zero, value);

            using var guard = Control.Guard();

            _sizeInterval = value;
        }
    }

    private readonly object _sizeLock = new();

    private readonly object _signalLock = new();

    private readonly object _rawLock = new();

    private readonly ManualResetEventSlim _resizeEvent = new();

    private readonly HashSet<ChildProcess> _processes = [];

    private Action<Size>? _resized;

    private Action<TerminalSignalContext>? _signaled;

    private PosixSignalRegistration? _sigHup;

    private PosixSignalRegistration? _sigInt;

    private PosixSignalRegistration? _sigQuit;

    private PosixSignalRegistration? _sigTerm;

    private Size? _size;

    private TimeSpan _sizeInterval = TimeSpan.FromMilliseconds(100);

    private protected SystemVirtualTerminal()
    {
        // Try to get the terminal size as early as reasonably possible.
        RefreshSize();

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
    }

    private protected abstract Size? QuerySize();

    private protected void RefreshSize()
    {
        if (QuerySize() is not { } size)
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
        _ = ThreadPool.UnsafeQueueUserWorkItem(
            static tup => Unsafe.As<SystemVirtualTerminal>(tup.This)._resized?.Invoke(tup.Size),
            (This: this, Size: size),
            preferLocal: true);
    }

    private protected abstract bool GetMode();

    private protected abstract void SetMode(bool raw, bool flush);

    private protected void ChangeRawMode(bool raw, bool flush, Action<SystemVirtualTerminal>? check)
    {
        lock (_rawLock)
        {
            check?.Invoke(this);

            SetMode(raw, flush);
        }
    }

    public override sealed void EnableRawMode()
    {
        using var guard = Control.Guard();

        ChangeRawMode(
            raw: true,
            flush: true,
            static @this => Check.Operation(
                @this._processes.Count == 0, $"Cannot enable raw mode with non-redirected child processes running."));
    }

    public override sealed void DisableRawMode()
    {
        using var guard = Control.Guard();

        ChangeRawMode(
            raw: false,
            flush: true,
            static @this => Check.Operation(
                @this._processes.Count == 0, $"Cannot disable raw mode with non-redirected child processes running."));
    }

    internal void StartProcess(Func<ChildProcess> starter)
    {
        lock (_rawLock)
        {
            // The vast majority of programs expect to start in cooked mode. Enforce that we are in cooked mode while
            // any child processes that could be using the terminal are running.
            Check.Operation(!IsRawMode, $"Cannot start non-redirected child processes in raw mode.");

            // Guard here since this locks us into cooked mode until all non-redirected processes are gone.
            using var guard = Control.Guard();

            _ = _processes.Add(starter());
        }
    }

    internal void ReapProcess(ChildProcess process)
    {
        lock (_rawLock)
        {
            _ = _processes.Remove(process);

            if (_processes.Count != 0)
                return;

            try
            {
                // Child processes may have messed up the terminal settings. Restore them just in case.
                SetMode(raw: false, flush: true);
            }
            catch (Exception e) when (e is TerminalNotAttachedException or TerminalConfigurationException)
            {
            }
        }
    }
}
