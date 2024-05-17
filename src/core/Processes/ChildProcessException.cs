// SPDX-License-Identifier: 0BSD

namespace Vezel.Cathode.Processes;

public class ChildProcessException : Exception
{
    public ChildProcessException()
        : this("An unknown child process exception occurred.")
    {
    }

    public ChildProcessException(string? message)
        : base(message)
    {
    }

    public ChildProcessException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
