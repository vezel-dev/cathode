namespace Cathode.IO;

public class TerminalConfigurationException : TerminalException
{
    public TerminalConfigurationException()
        : this("An unknown terminal configuration error occurred.")
    {
    }

    public TerminalConfigurationException(string? message)
        : base(message)
    {
    }

    public TerminalConfigurationException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
