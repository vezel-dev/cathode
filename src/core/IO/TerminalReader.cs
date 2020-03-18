using System.Drivers;

namespace System.IO
{
    public abstract class TerminalReader : TerminalHandle
    {
        public TerminalInputStream Stream { get; }

        private protected TerminalReader(TerminalDriver driver)
            : base(driver)
        {
            Stream = new TerminalInputStream(this);
        }

        public abstract int Read(Span<byte> data);
    }
}
