namespace System.Processes;

public class ChildProcessException : Exception
{
    public int ExitCode { get; }

    public ChildProcessException()
        : this("An unknown child process error occurred.")
    {
    }

    public ChildProcessException(string? message)
        : base(message)
    {
    }

    public ChildProcessException(string? message, int exitCode)
        : this(message)
    {
        ExitCode = exitCode;
    }

    public ChildProcessException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
