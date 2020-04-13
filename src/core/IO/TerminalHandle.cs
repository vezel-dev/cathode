using System.Drivers;
using System.Text;

namespace System.IO
{
    public abstract class TerminalHandle
    {
        internal TerminalDriver Driver { get; }

#pragma warning disable CA1822

        public Encoding Encoding => TerminalDriver.Encoding;

#pragma warning restore CA1822

        public abstract bool IsRedirected { get; }

        private protected TerminalHandle(TerminalDriver driver)
        {
            Driver = driver;
        }
    }
}
