namespace System.Diagnostics;

public sealed class TerminalProcessThread
{
    public int Id => _thread.Id;

    public DateTime StartTime => _thread.StartTime;

    public TimeSpan UserTime => _thread.UserProcessorTime;

    public TimeSpan KernelTime => _thread.PrivilegedProcessorTime;

    public TimeSpan TotalTime => _thread.TotalProcessorTime;

    public ThreadState State => _thread.ThreadState;

    readonly ProcessThread _thread;

    internal TerminalProcessThread(ProcessThread thread)
    {
        _thread = thread;
    }
}
