namespace System.Drivers;

interface IUnixTerminalInterop
{
    TerminalSize? Size { get; }

    void RefreshSettings();

    bool SetRawMode(bool raw, bool discard);
}
