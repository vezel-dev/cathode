namespace Vezel.Cathode.IO;

public class TerminalException : IOException
{
    public TerminalException()
        : this("An unknown terminal exception occurred.")
    {
    }

    public TerminalException(string? message)
        : base(message)
    {
    }

    public TerminalException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
