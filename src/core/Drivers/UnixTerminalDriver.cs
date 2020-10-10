using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Mono.Unix;
using Mono.Unix.Native;

namespace System.Drivers
{
    sealed class UnixTerminalDriver : TerminalDriver
    {
        sealed class UnixTerminalReader : TerminalReader
        {
            public int Handle { get; }

            public override bool IsRedirected => IsRedirected(Handle);

            readonly object _lock = new();

            readonly string _name;

            public UnixTerminalReader(TerminalDriver driver, int handle, string name)
                : base(driver)
            {
                Handle = handle;
                _name = name;
            }

            public override unsafe int Read(Span<byte> data)
            {
                if (data.IsEmpty)
                    return 0;

                long ret;

                lock (_lock)
                {
                    while (true)
                    {
                        fixed (byte* p = data)
                        {
                            while ((ret = Syscall.read(Handle, p, (ulong)data.Length)) == -1 &&
                                Stdlib.GetLastError() == Errno.EINTR)
                            {
                                // Retry in case we get interrupted by a signal.
                            }

                            if (ret != -1)
                                break;

                            var err = Stdlib.GetLastError();

                            // The descriptor was probably redirected to a program that ended. Just
                            // silently ignore this situation.
                            //
                            // The strange condition where errno is zero happens e.g. on Linux if
                            // the process is killed while blocking in the read system call.
                            if (err == 0 || err == Errno.EPIPE)
                            {
                                ret = 0;

                                break;
                            }

                            // The file descriptor has been configured as non-blocking. Instead of
                            // busily trying to read over and over, poll until we can write and then
                            // try again.
                            if (err == Errno.EAGAIN)
                            {
                                _ = Syscall.poll(new[]
                                {
                                    new Pollfd
                                    {
                                        fd = Handle,
                                        events = PollEvents.POLLIN,
                                    },
                                }, 1, Timeout.Infinite);

                                continue;
                            }

                            if (err == 0)
                                err = Errno.EBADF;

                            throw new TerminalException(
                                $"Could not read from standard {_name}: {Stdlib.strerror(err)}");
                        }
                    }
                }

                return (int)ret;
            }
        }

        sealed class UnixTerminalWriter : TerminalWriter
        {
            public int Handle { get; }

            public override bool IsRedirected => IsRedirected(Handle);

            readonly object _lock = new();

            readonly string _name;

            public UnixTerminalWriter(TerminalDriver driver, int handle, string name)
                : base(driver)
            {
                Handle = handle;
                _name = name;
            }

            public override unsafe void Write(ReadOnlySpan<byte> data)
            {
                if (data.IsEmpty)
                    return;

                lock (_lock)
                {
                    var progress = 0;

                    fixed (byte* p = data)
                    {
                        var len = data.Length;

                        while (progress < len)
                        {
                            long ret;

                            while ((ret = Syscall.write(Handle, p + progress, (ulong)(len - progress))) == -1 &&
                                Stdlib.GetLastError() == Errno.EINTR)
                            {
                            }

                            // The descriptor has been closed by someone else. Just silently ignore
                            // this situation.
                            if (ret == 0)
                                break;

                            if (ret != -1)
                            {
                                progress += (int)ret;

                                continue;
                            }

                            var err = Stdlib.GetLastError();

                            // The descriptor was probably redirected to a program that ended. Just
                            // silently ignore this situation.
                            if (err == Errno.EPIPE)
                                break;

                            // The file descriptor has been configured as non-blocking. Instead of
                            // busily trying to write over and over, poll until we can write and
                            // then try again.
                            if (err == Errno.EAGAIN)
                            {
                                _ = Syscall.poll(new[]
                                {
                                    new Pollfd
                                    {
                                        fd = Handle,
                                        events = PollEvents.POLLOUT,
                                    },
                                }, 1, Timeout.Infinite);

                                continue;
                            }

                            throw new TerminalException($"Could not write to standard {_name}: {Stdlib.strerror(err)}");
                        }
                    }
                }
            }
        }

        public const int InHandle = 0;

        public const int OutHandle = 1;

        public const int ErrorHandle = 2;

        public static UnixTerminalDriver Instance { get; } = new();

        public override TerminalReader StdIn { get; }

        public override TerminalWriter StdOut { get; }

        public override TerminalWriter StdError { get; }

        public override TerminalSize Size => _size switch
        {
            TerminalSize s => s,
            _ => throw new TerminalException("There is no terminal attached."),
        };

        readonly IUnixTerminalInterop _interop = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ?
            (IUnixTerminalInterop)LinuxTerminalInterop.Instance : OSXTerminalInterop.Instance;

        readonly object _rawLock = new();

        TerminalSize? _size;

        UnixTerminalDriver()
        {
            StdIn = new UnixTerminalReader(this, InHandle, "input");
            StdOut = new UnixTerminalWriter(this, OutHandle, "output");
            StdError = new UnixTerminalWriter(this, ErrorHandle, "error");

            void RefreshWindowSize()
            {
                if (_interop.Size is TerminalSize s)
                {
                    _size = s;

                    // We currently trust that SIGWINCH will always be delivered when the terminal
                    // size changes. If this ever turns out to be false somewhere/somehow, we may
                    // need to use a background thread to also poll for size changes like in the
                    // Windows driver.
                    HandleResize(s);
                }
            }

            RefreshWindowSize();

            _ = TerminalUtility.StartThread("Terminal Signal Listener", () =>
            {
                // TODO: SIGCHLD?

                using var sigWinch = new UnixSignal(Signum.SIGWINCH);
                using var sigCont = new UnixSignal(Signum.SIGCONT);
                using var sigInt = new UnixSignal(Signum.SIGINT);
                using var sigQuit = new UnixSignal(Signum.SIGQUIT);

                var sigs = new[] { sigWinch, sigCont, sigInt, sigQuit };

                while (true)
                {
                    var idx = UnixSignal.WaitAny(sigs);

                    if (idx == -1)
                        break;

                    var sig = sigs[idx];

                    // If we are being restored from the background (SIGCONT), it is possible that
                    // terminal settings have been mangled, so restore them.
                    if (sig == sigCont)
                        lock (_rawLock)
                            _interop.RefreshSettings();

                    // Terminal width/height might have changed for SIGCONT, and will definitely
                    // have changed for SIGWINCH.
                    if (sig == sigCont || sig == sigWinch)
                        RefreshWindowSize();

                    if (sig == sigQuit || sig == sigInt)
                    {
                        // We do this in a separate thread so that signal handling does not get
                        // blocked if an event handler misbehaves.
                        _ = TerminalUtility.StartThread("Terminal Break Handler", () =>
                        {
                            if (!HandleBreakSignal(sig == sigInt))
                            {
                                // Get the value early to avoid ObjectDisposedException.
                                var num = sig.Signum;

                                // Remove our signal handler and send the signal again. Since we
                                // have overwritten the signal handlers in CoreCLR and
                                // System.Native, this gives those handlers an opportunity to run.
                                sig.Dispose();
                                _ = Syscall.kill(Syscall.getpid(), num);
                            }
                        });
                    }
                }
            });
        }

        public override void GenerateBreakSignal(TerminalBreakSignal signal)
        {
            _ = Syscall.kill(0, signal switch
            {
                TerminalBreakSignal.Interrupt => Signum.SIGINT,
                TerminalBreakSignal.Quit => Signum.SIGQUIT,
                _ => throw new ArgumentOutOfRangeException(nameof(signal)),
            });
        }

        public override void GenerateSuspendSignal()
        {
            _ = Syscall.kill(0, Signum.SIGTSTP);
        }

        static bool IsRedirected(int handle)
        {
            return !Syscall.isatty(handle);
        }

        protected override void SetRawModeCore(bool raw, bool discard)
        {
            lock (_rawLock)
                if (!_interop.SetRawMode(raw, discard))
                    throw new TerminalException("There is no terminal attached.");
        }
    }
}
