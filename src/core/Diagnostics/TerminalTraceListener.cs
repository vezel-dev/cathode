namespace Vezel.Cathode.Diagnostics;

public class TerminalTraceListener : TextWriterTraceListener
{
    public TerminalTraceListener(TerminalWriter writer)
        : base((writer ?? throw new ArgumentNullException(nameof(writer))).TextWriter)
    {
    }
}
