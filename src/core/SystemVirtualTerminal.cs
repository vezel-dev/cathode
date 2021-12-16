using System.Drivers;
using System.Drivers.Unix;
using System.Drivers.Windows;

namespace System;

public sealed class SystemVirtualTerminal : VirtualTerminal
{
    public override event Action<TerminalSize>? Resized
    {
        add => _driver.Resized += value;
        remove => _driver.Resized -= value;
    }

    public override event Action<TerminalSignalContext>? Signaled
    {
        add => _driver.Signaled += value;
        remove => _driver.Signaled -= value;
    }

    public static SystemVirtualTerminal Instance { get; } = new();

    public override TerminalReader StandardIn => _driver.StandardIn;

    public override TerminalWriter StandardOut => _driver.StandardOut;

    public override TerminalWriter StandardError => _driver.StandardError;

    public override TerminalReader TerminalIn => _driver.TerminalIn;

    public override TerminalWriter TerminalOut => _driver.TerminalOut;

    public override bool IsRawMode => _driver.IsRawMode;

    public override TerminalSize Size => _driver.Size;

    public TimeSpan SizePollingInterval
    {
        get => _driver.SizePollingInterval;
        set => _driver.SizePollingInterval = value;
    }

    readonly TerminalDriver _driver =
        OperatingSystem.IsLinux() ? LinuxTerminalDriver.Instance :
        OperatingSystem.IsMacOS() ? MacOSTerminalDriver.Instance :
        OperatingSystem.IsWindows() ? WindowsTerminalDriver.Instance :
        throw new PlatformNotSupportedException();

    SystemVirtualTerminal()
    {
    }

    public override void GenerateSignal(TerminalSignal signal)
    {
        _driver.SendSignal(0, signal);
    }

    public override void EnableRawMode()
    {
        _driver.EnableRawMode();
    }

    public override void DisableRawMode()
    {
        _driver.DisableRawMode();
    }

    internal void StartProcess(Func<TerminalProcess> starter)
    {
        _driver.StartProcess(starter);
    }

    internal void SignalProcess(TerminalProcess process, TerminalSignal signal)
    {
        _driver.SendSignal(process.Id, signal);
    }

    internal void ReapProcess(TerminalProcess process)
    {
        _driver.ReapProcess(process);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void DangerousRestoreSettings()
    {
        // This method is named as it is and hidden from IntelliSense because restoring the original terminal settings
        // and then calling any I/O method in our API could have completely unpredictable results. Nevertheless, we
        // expose the method for people who know what they are doing.

        _driver.RestoreSettings();
    }
}
