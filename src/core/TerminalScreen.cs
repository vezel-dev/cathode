using System.Drivers;
using static System.TerminalConstants;

namespace System
{
    public sealed class TerminalScreen
    {
        public readonly struct ScreenActivator : IDisposable
        {
            public TerminalScreen NewScreen { get; }

            public TerminalScreen OldScreen { get; }

            internal ScreenActivator(TerminalScreen screen)
            {
                NewScreen = screen;
                OldScreen = Terminal.Screen;

                Switch(screen);
            }

            public void Dispose()
            {
                // We might be default-initialized.
                if (OldScreen != null)
                    Switch(OldScreen);
            }

            static void Switch(TerminalScreen screen)
            {
                lock (_lock)
                {
                    screen._driver.Sequence($"{CSI}?1049{(screen.IsMain ? 'l' : 'h')}");

                    Terminal.Screen = screen;
                }
            }
        }

        public bool IsMain { get; }

        public bool IsActive => Terminal.Screen == this;

        public bool IsCursorVisible
        {
            get => _visible;
            set
            {
                lock (_lock)
                {
                    CheckActive();

                    _driver.Sequence($"{CSI}?25{(value ? 'h' : 'l')}");

                    _visible = value;
                }
            }
        }

        public TerminalCursorStyle CursorStyle
        {
            get => _style;
            set
            {
                var type = value switch
                {
                    TerminalCursorStyle.Default => '0',
                    TerminalCursorStyle.BlockStatic => '2',
                    TerminalCursorStyle.BlockBlinking => '1',
                    TerminalCursorStyle.UnderlineStatic => '4',
                    TerminalCursorStyle.UnderlineBlinking => '3',
                    TerminalCursorStyle.BarStatic => '6',
                    TerminalCursorStyle.BarBlinking => '5',
                    _ => throw new ArgumentOutOfRangeException(nameof(value)),
                };

                lock (_lock)
                {
                    CheckActive();

                    _driver.Sequence($"{CSI}{type} q");

                    _style = value;
                }
            }
        }

        static readonly object _lock = new();

        readonly TerminalDriver _driver;

        bool _visible = true;

        TerminalCursorStyle _style;

        internal TerminalScreen(TerminalDriver driver, bool main)
        {
            IsMain = main;
            _driver = driver;
        }

        public ScreenActivator Activate()
        {
            return new(this);
        }

        void CheckActive()
        {
            if (!IsActive)
                throw new InvalidOperationException("This screen is inactive.");
        }
    }
}
