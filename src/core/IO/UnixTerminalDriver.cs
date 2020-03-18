using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Mono.Unix;
using Mono.Unix.Native;

namespace System.IO
{
    sealed class UnixTerminalDriver : ITerminalDriver
    {
        abstract class UnixTerminalHandle : ITerminalHandle
        {
            public int Handle { get; }

            public Encoding Encoding { get; }

            public bool IsRedirected => !Syscall.isatty(Handle);

            protected string Name { get; }

            protected object Lock { get; } = new object();

            protected UnixTerminalHandle(int handle, Encoding encoding, string name)
            {
                Handle = handle;
                Encoding = encoding;
                Name = name;
            }
        }

        sealed class UnixTerminalReader : UnixTerminalHandle, ITerminalReader
        {
            public TerminalInputStream Stream { get; }

            public UnixTerminalReader(int handle, Encoding encoding, string name)
                : base(handle, encoding, name)
            {
                Stream = new TerminalInputStream(this);
            }

            public unsafe int Read(Span<byte> data)
            {
                if (data.IsEmpty)
                    return 0;

                long ret;

                lock (Lock)
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
                                $"Could not read from standard {Name}: {Stdlib.strerror(err)}");
                        }
                    }
                }

                return (int)ret;
            }
        }

        sealed class UnixTerminalWriter : UnixTerminalHandle, ITerminalWriter
        {
            public TerminalOutputStream Stream { get; }

            public UnixTerminalWriter(int handle, Encoding encoding, string name)
                : base(handle, encoding, name)
            {
                Stream = new TerminalOutputStream(this);
            }

            public unsafe void Write(ReadOnlySpan<byte> data)
            {
                if (data.IsEmpty)
                    return;

                lock (Lock)
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

                            throw new TerminalException($"Could not write to standard {Name}: {Stdlib.strerror(err)}");
                        }
                    }
                }
            }
        }

        public const int InHandle = 0;

        public const int OutHandle = 1;

        public const int ErrorHandle = 2;

        public static UnixTerminalDriver Instance { get; } = new UnixTerminalDriver();

        public ITerminalReader StdIn { get; } =
            new UnixTerminalReader(InHandle, TerminalUtility.Encoding, "input");

        public ITerminalWriter StdOut { get; } =
            new UnixTerminalWriter(OutHandle, TerminalUtility.Encoding, "output");

        public ITerminalWriter StdError { get; } =
            new UnixTerminalWriter(ErrorHandle, TerminalUtility.Encoding, "error");

        public int Width { get; private set; } = TerminalUtility.InvalidSize;

        public int Height { get; private set; } = TerminalUtility.InvalidSize;

        readonly IUnixTerminalInterop _interop = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ?
            (IUnixTerminalInterop)LinuxTerminalInterop.Instance : OSXTerminalInterop.Instance;

        readonly object _rawLock = new object();

        UnixTerminalDriver()
        {
            void RefreshWindowSize()
            {
                if (_interop.WindowSize is (int w, int h))
                {
                    Width = w;
                    Height = h;
                }
            }

            RefreshWindowSize();

            TerminalUtility.StartThread("Terminal Signal Listener", () =>
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
                        TerminalUtility.StartThread("Terminal Break Handler", () =>
                        {
                            if (!Terminal.HandleBreak(sig == sigInt))
                            {
                                // Remove our signal handler and send the signal again. Since we
                                // have overwritten the signal handlers in CoreCLR and
                                // System.Native, this gives those handlers an opportunity to run.
                                sig.Dispose();
                                _ = Syscall.kill(Syscall.getpid(), sig.Signum);
                            }
                        });
                    }
                }
            });
        }

        public void SetRawMode(bool raw, bool discard)
        {
            lock (_rawLock)
                if (!_interop.SetRawMode(raw, discard))
                    throw new TerminalException("There is no terminal attached.");
        }
    }
}
