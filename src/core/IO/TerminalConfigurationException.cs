// SPDX-License-Identifier: 0BSD

namespace Vezel.Cathode.IO;

public class TerminalConfigurationException : TerminalException
{
    public TerminalConfigurationException()
        : this("An unknown terminal configuration exception occurred.")
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
