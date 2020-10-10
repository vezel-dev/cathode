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

        public bool IsCursorBlinking
        {
            get => _blinking;
            set
            {
                lock (_lock)
                {
                    CheckActive();

                    _driver.Sequence($"{CSI}?12{(value ? 'h' : 'l')}");

                    _blinking = value;
                }
            }
        }

        static readonly object _lock = new object();

        readonly TerminalDriver _driver;

        bool _visible = true;

        bool _blinking = true;

        internal TerminalScreen(TerminalDriver driver, bool main)
        {
            IsMain = main;
            _driver = driver;
        }

        public ScreenActivator Activate()
        {
            return new ScreenActivator(this);
        }

        void CheckActive()
        {
            if (!IsActive)
                throw new InvalidOperationException("This screen is inactive.");
        }
    }
}
