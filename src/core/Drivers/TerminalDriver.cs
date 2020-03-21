using System.Collections.Generic;
using System.IO;
using System.Text;
using static System.TerminalConstants;

namespace System.Drivers
{
    abstract class TerminalDriver
    {
        const int ReadBufferSize = 4096;

        public event EventHandler<TerminalBreakSignalEventArgs>? BreakSignal;

        public static Encoding Encoding { get; } = Encoding.UTF8;

        public abstract TerminalReader StdIn { get; }

        public abstract TerminalWriter StdOut { get; }

        public abstract TerminalWriter StdError { get; }

        public bool IsRawMode { get; private set; }

        public string Title
        {
            get => _title;
            set
            {
                _ = value ?? throw new ArgumentNullException(nameof(value));

                lock (_lock)
                {
                    Sequence($"{OSC}0;{value}{BEL}");

                    _title = value;
                }
            }
        }

        public abstract (int Width, int Height) Size { get; }

        public TerminalKeyMode CursorKeyMode
        {
            get => _cursor;
            set
            {
                var type = value switch
                {
                    TerminalKeyMode.Normal => 'l',
                    TerminalKeyMode.Application => 'h',
                    _ => throw new ArgumentOutOfRangeException(nameof(value)),
                };

                lock (_lock)
                {
                    Sequence($"{CSI}?1{type}");

                    _cursor = value;
                }
            }
        }

        public TerminalKeyMode NumericKeyMode
        {
            get => _numeric;
            set
            {
                var type = value switch
                {
                    TerminalKeyMode.Normal => '>',
                    TerminalKeyMode.Application => '=',
                    _ => throw new ArgumentOutOfRangeException(nameof(value)),
                };

                lock (_lock)
                {
                    Sequence($"{ESC}{type}");

                    _numeric = value;
                }
            }
        }

        public bool IsGraphicsEnabled
        {
            get => _graphics;
            set
            {
                lock (_lock)
                {
                    Sequence($"{ESC}({(value ? '0' : 'B')}");

                    _graphics = value;
                }
            }
        }

        readonly object _lock = new object();

        readonly Lazy<StreamReader> _reader;

        string _title = string.Empty;

        TerminalKeyMode _cursor;

        TerminalKeyMode _numeric;

        bool _graphics;

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

        public abstract void GenerateBreakSignal(TerminalBreakSignal signal);

        protected bool HandleBreakSignal(bool interrupt)
        {
            var args = new TerminalBreakSignalEventArgs(interrupt ?
                TerminalBreakSignal.Interrupt : TerminalBreakSignal.Quit);

            BreakSignal?.Invoke(null, args);

            return args.Cancel;
        }

        public abstract void GenerateSuspendSignal();

        public void SetRawMode(bool raw, bool discard)
        {
            lock (_lock)
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

        public void Sequence(string value)
        {
            if (!StdOut.IsRedirected)
                StdOut.Write(value);
            else if (!StdError.IsRedirected)
                StdError.Write(value);
        }

        public void Beep()
        {
            Sequence(BEL);
        }

        public void Insert(int count)
        {
            Sequence($"{CSI}{count}@");
        }

        public void Delete(int count)
        {
            Sequence($"{CSI}{count}P");
        }

        public void Erase(int count)
        {
            Sequence($"{CSI}{count}X");
        }

        public void InsertLine(int count)
        {
            Sequence($"{CSI}{count}L");
        }

        public void DeleteLine(int count)
        {
            Sequence($"{CSI}{count}M");
        }

        void Clear(char type, TerminalClearMode mode)
        {
            var m = mode switch
            {
                TerminalClearMode.Before => 1,
                TerminalClearMode.After => 0,
                TerminalClearMode.Full => 2,
                _ => throw new ArgumentOutOfRangeException(nameof(mode)),
            };

            Sequence($"{CSI}{m}{type}");
        }

        public void ClearScreen(TerminalClearMode mode)
        {
            Clear('J', mode);
        }

        public void ClearLine(TerminalClearMode mode)
        {
            Clear('K', mode);
        }

        public void SetScrollRegion(int top, int bottom)
        {
            _ = top >= 0 ? true : throw new ArgumentOutOfRangeException(nameof(top));
            _ = bottom >= 0 ? true : throw new ArgumentOutOfRangeException(nameof(bottom));

            var (_, height) = Size;

            // Most terminals will clamp the values so that there are at least
            // 2 lines of scrollable text: One for the last line written and one
            // for the current line. Enforce this. We throw TerminalException
            // instead of ArgumentException since the Size property could change
            // between the caller observing it and calling this method.
            if (height - top - bottom < 2)
                throw new TerminalException("Scroll region is too small.");

            Sequence($"{CSI}{top + 1};{height - bottom}r");
        }

        void MoveBuffer(char type, int count)
        {
            _ = count >= 0 ? true : throw new ArgumentOutOfRangeException(nameof(count));

            if (count != 0)
                Sequence($"{CSI}{count}{type}");
        }

        public void MoveBufferUp(int count)
        {
            MoveBuffer('S', count);
        }

        public void MoveBufferDown(int count)
        {
            MoveBuffer('T', count);
        }

        public void MoveCursorTo(int row, int column)
        {
            _ = row >= 0 ? true : throw new ArgumentOutOfRangeException(nameof(row));
            _ = column >= 0 ? true : throw new ArgumentOutOfRangeException(nameof(column));

            Sequence($"{CSI}{column + 1};{row + 1}H");
        }

        void MoveCursor(char type, int count)
        {
            _ = count >= 0 ? true : throw new ArgumentOutOfRangeException(nameof(count));

            if (count != 0)
                Sequence($"{CSI}{count}{type}");
        }

        public void MoveCursorUp(int count)
        {
            MoveCursor('A', count);
        }

        public void MoveCursorDown(int count)
        {
            MoveCursor('B', count);
        }

        public void MoveCursorLeft(int count)
        {
            MoveCursor('D', count);
        }

        public void MoveCursorRight(int count)
        {
            MoveCursor('C', count);
        }

        public void SaveCursorPosition()
        {
            Sequence($"{ESC}7");
        }

        public void RestoreCursorPosition()
        {
            Sequence($"{ESC}8");
        }

        public void ForegroundColor(byte r, byte g, byte b)
        {
            Sequence($"{CSI}38;2;{r};{g};{b}m");
        }

        public void BackgroundColor(byte r, byte g, byte b)
        {
            Sequence($"{CSI}48;2;{r};{g};{b}m");
        }

        public void Decorations(bool bold = false, bool faint = false, bool italic = false, bool underline = false,
            bool blink = false, bool invert = false, bool invisible = false, bool strike = false)
        {
            // TODO: Avoid the allocations here, even if the code gets a bit ugly.

            var dict = new Dictionary<byte, bool>
            {
                [1] = bold,
                [2] = faint,
                [3] = italic,
                [4] = underline,
                [5] = blink,
                [7] = invert,
                [8] = invisible,
                [9] = strike,
            };
            var seqs = new List<string>(dict.Count);

            foreach (var (code, enabled) in dict)
                if (enabled)
                    seqs.Add(code.ToString());

            Sequence($"{CSI}{string.Join(';', seqs)}m");
        }

        public void ResetAttributes()
        {
            Sequence($"{CSI}0m");
        }

        public void ResetAll()
        {
            Sequence($"{CSI}!p");
        }
    }
}
