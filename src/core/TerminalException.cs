namespace System;

[Serializable]
public class TerminalException : IOException
{
    public TerminalException()
        : this("An unspecified terminal error occurred.")
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

    protected TerminalException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
