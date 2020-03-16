using System.Text;

namespace System.IO
{
    public interface ITerminalHandle
    {
        Encoding Encoding { get; }

        bool IsRedirected { get; }
    }
}
