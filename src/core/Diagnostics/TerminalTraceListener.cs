using Vezel.Cathode.IO;

namespace Vezel.Cathode.Diagnostics;

public class TerminalTraceListener : TextWriterTraceListener
{
    public TerminalTraceListener(TerminalWriter writer)
        : base(writer.TextWriter)
    {
    }
}
