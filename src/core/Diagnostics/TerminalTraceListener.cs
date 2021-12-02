namespace System.Diagnostics;

public class TerminalTraceListener : TextWriterTraceListener
{
    public TerminalTraceListener(bool stdError = false)
        : base((stdError ? Terminal.StandardError : Terminal.StandardOut).Stream)
    {
    }
}
