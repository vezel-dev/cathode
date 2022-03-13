namespace Cathode.IO;

public class TerminalNotAttachedException : TerminalException
{
    public TerminalNotAttachedException()
        : this("There is no terminal attached.")
    {
    }

    public TerminalNotAttachedException(string? message)
        : base(message)
    {
    }

    public TerminalNotAttachedException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
