namespace Cathode.Diagnostics;

public class TerminalTraceListener : TextWriterTraceListener
{
    public TerminalTraceListener(TerminalWriter writer)
        : base(writer.TextWriter)
    {
    }
}
