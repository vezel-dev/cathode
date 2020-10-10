using System.Diagnostics.CodeAnalysis;
using System.Drivers;
using System.Text;

namespace System.IO
{
    public abstract class TerminalHandle
    {
        internal TerminalDriver Driver { get; }

        [SuppressMessage("Microsoft.Performance", "CA1822", Justification = "Intentional.")]
        public Encoding Encoding => TerminalDriver.Encoding;

        public abstract bool IsRedirected { get; }

        private protected TerminalHandle(TerminalDriver driver)
        {
            Driver = driver;
        }
    }
}
