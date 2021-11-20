namespace System.IO;

public abstract class TerminalHandle
{
    internal TerminalDriver Driver { get; }

    [SuppressMessage("Performance", "CA1822")]
    public Encoding Encoding => TerminalDriver.Encoding;

    public abstract bool IsRedirected { get; }

    private protected TerminalHandle(TerminalDriver driver)
    {
        Driver = driver;
    }
}
