using System.Drivers;

namespace System.IO
{
    public abstract class TerminalWriter : TerminalHandle
    {
        public TerminalOutputStream Stream { get; }

        private protected TerminalWriter(TerminalDriver driver)
            : base(driver)
        {
            Stream = new TerminalOutputStream(this);
        }

        public abstract void Write(ReadOnlySpan<byte> data);
    }
}
