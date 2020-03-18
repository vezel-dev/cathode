namespace System.Drivers
{
    interface IUnixTerminalInterop
    {
        (int Width, int Height)? Size { get; }

        void RefreshSettings();

        bool SetRawMode(bool raw, bool discard);
    }
}
