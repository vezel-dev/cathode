namespace System.Drivers;

abstract class DriverTerminalReader<TDriver, THandle> : TerminalReader
    where TDriver : TerminalDriver<THandle>
{
    public TDriver Driver { get; }

    public string Name { get; }

    public THandle Handle { get; }

    public override sealed TerminalInputStream Stream { get; }

    public override sealed bool IsValid { get; }

    public override sealed bool IsInteractive { get; }

    protected DriverTerminalReader(TDriver driver, string name, THandle handle)
    {
        Driver = driver;
        Name = name;
        Handle = handle;
        Stream = new(this);
        IsValid = driver.IsHandleValid(handle, false);
        IsInteractive = driver.IsHandleInteractive(handle);
    }
}
