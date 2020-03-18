using System.IO;
using System.Text;
using static System.TerminalConstants;

namespace System.Drivers
{
    abstract class TerminalDriver
    {
        const int ReadBufferSize = 4096;

        public event EventHandler<TerminalBreakEventArgs>? Break;

        public static Encoding Encoding { get; } = Encoding.UTF8;

        public abstract TerminalReader StdIn { get; }

        public abstract TerminalWriter StdOut { get; }

        public abstract TerminalWriter StdError { get; }

        public bool IsRawMode { get; private set; }

        public abstract (int Width, int Height) Size { get; }

        public string Title
        {
            get => _title;
            set
            {
                _ = value ?? throw new ArgumentNullException(nameof(value));

                lock (_titleLock)
                {
                    Sequence($"{OSC}0;{value}\a");

                    _title = value;
                }
            }
        }

        readonly object _titleLock = new object();

        readonly object _rawLock = new object();

        readonly Lazy<StreamReader> _reader;

        string _title = string.Empty;

        protected TerminalDriver()
        {
            _reader = new Lazy<StreamReader>(
                () => new StreamReader(StdIn.Stream, StdIn.Encoding, false, ReadBufferSize, true));

            // Accessing this property has no particularly important effect on Windows, but it does
            // do something important on Unix: If a terminal is attached, it will force Console to
            // initialize its System.Native portions, which includes signal handlers and terminal
            // settings. Thus, by doing this here, we ensure that if a user accidentally accesses
            // Console at some point later, it will not overwrite our signal handlers or terminal
            // settings.
            _ = Console.In;

            // Try to prevent Console/Terminal intermixing from breaking stuff. This should prevent
            // basic read/write calls on Console from calling into internal classes like ConsolePal
            // and StdInReader (which in turn call System.Native functions that, among other things,
            // change terminal settings).
            //
            // There are still many problematic properties and methods beyond these, but there is
            // not much we can do about those.
            Console.SetIn(new InvalidTextReader());
            Console.SetOut(new InvalidTextWriter());
            Console.SetError(new InvalidTextWriter());
        }

        protected bool HandleBreak(bool interrupt)
        {
            var args = new TerminalBreakEventArgs(interrupt ? TerminalBreakKey.Interrupt : TerminalBreakKey.Quit);

            Break?.Invoke(null, args);

            return args.Cancel;
        }

        public void SetRawMode(bool raw, bool discard)
        {
            lock (_rawLock)
            {
                SetRawModeCore(raw, discard);

                IsRawMode = raw;
            }
        }

        protected abstract void SetRawModeCore(bool raw, bool discard);

        public unsafe byte? ReadRaw()
        {
            byte value;

            return StdIn.Stream.Read(new Span<byte>(&value, 1)) == 1 ? value : default;
        }

        public string? ReadLine()
        {
            return _reader.Value.ReadLine();
        }

        void Sequence(string value)
        {
            if (!StdOut.IsRedirected)
                StdOut.Write(value);
            else if (!StdError.IsRedirected)
                StdError.Write(value);
        }

        public void Clear(bool cursor)
        {
            Sequence($"{CSI}2J{(cursor ? $"{CSI}H" : string.Empty)}");
        }

        public void Beep()
        {
            Sequence("\a");
        }
    }
}
