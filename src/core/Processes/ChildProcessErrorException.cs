// SPDX-License-Identifier: 0BSD

namespace Vezel.Cathode.Processes;

public class ChildProcessErrorException : ChildProcessException
{
    public int ExitCode { get; }

    public ChildProcessErrorException()
        : this("An unknown child process error occurred.")
    {
    }

    public ChildProcessErrorException(string? message)
        : base(message)
    {
    }

    public ChildProcessErrorException(string? message, int exitCode)
        : this(message)
    {
        ExitCode = exitCode;
    }

    public ChildProcessErrorException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
