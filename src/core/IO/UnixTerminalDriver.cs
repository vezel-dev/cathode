using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Mono.Unix;
using Mono.Unix.Native;

namespace System.IO
{
    sealed class UnixTerminalDriver : ITerminalDriver
    {
        sealed class UnixTerminalReader : ITerminalReader
        {
            public int Handle { get; }

            public Encoding Encoding { get; }

            public bool IsRedirected => IsRedirected(Handle);

            public TerminalInputStream Stream { get; }

            readonly object _lock = new object();

            readonly string _name;

            public UnixTerminalReader(int handle, Encoding encoding, string name)
            {
                Stream = new TerminalInputStream(this);
                Handle = handle;
                Encoding = encoding;
                _name = name;
            }

            public unsafe int Read(Span<byte> data)
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

        sealed class UnixTerminalWriter : ITerminalWriter
        {
            public int Handle { get; }

            public Encoding Encoding { get; }

            public bool IsRedirected => IsRedirected(Handle);

            public TerminalOutputStream Stream { get; }

            readonly object _lock = new object();

            readonly string _name;

            public UnixTerminalWriter(int handle, Encoding encoding, string name)
            {
                Stream = new TerminalOutputStream(this);
                Handle = handle;
                Encoding = encoding;
                _name = name;
            }

            public unsafe void Write(ReadOnlySpan<byte> data)
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

        public ITerminalReader StdIn { get; } =
            new UnixTerminalReader(InHandle, TerminalUtility.InEncoding, "input");

        public ITerminalWriter StdOut { get; } =
            new UnixTerminalWriter(OutHandle, TerminalUtility.OutEncoding, "output");

        public ITerminalWriter StdError { get; } =
            new UnixTerminalWriter(ErrorHandle, TerminalUtility.ErrorEncoding, "error");

        public int Width { get; private set; } = TerminalUtility.InvalidSize;

        public int Height { get; private set; } = TerminalUtility.InvalidSize;

        static readonly IUnixTerminalInterop _interop = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ?
            (IUnixTerminalInterop)new LinuxTerminalInterop() : new OSXTerminalInterop();

        readonly object _rawLock = new object();

        public UnixTerminalDriver()
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

            new Thread(() =>
            {
                using var winch = new UnixSignal(Signum.SIGWINCH);
                using var cont = new UnixSignal(Signum.SIGCONT);

                var sigs = new[] { winch, cont };

                while (!Environment.HasShutdownStarted)
                {
                    var idx = UnixSignal.WaitAny(sigs);

                    // If we are being restored from the background (SIGCONT), it is possible that
                    // terminal settings have been mangled, so restore them.
                    if (idx == 1)
                        lock (_rawLock)
                            _interop.RefreshSettings();

                    // Terminal width/height might have changed for SIGCONT, and will definitely
                    // have changed for SIGWINCH.
                    RefreshWindowSize();
                }
            })
            {
                IsBackground = true,
                Name = $"Terminal Signal Listener",
            }.Start();
        }

        static bool IsRedirected(int handle)
        {
            return !Syscall.isatty(handle);
        }

        public void SetRawMode(bool raw, bool discard)
        {
            lock (_rawLock)
                if (!_interop.SetRawMode(raw, discard))
                    throw new TerminalException("There is no terminal attached.");
        }
    }
}
