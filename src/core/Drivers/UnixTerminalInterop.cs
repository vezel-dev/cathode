namespace System.Drivers;

abstract class UnixTerminalInterop
{
    public abstract TerminalSize? Size { get; }

    public abstract void RefreshSettings();

    public abstract bool SetRawMode(bool raw, bool discard);
}
