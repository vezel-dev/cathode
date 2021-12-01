using System.Drivers;
using System.Drivers.Unix;
using System.Drivers.Windows;

namespace System;

public sealed class SystemVirtualTerminal : VirtualTerminal
{
    public override event Action<TerminalSize>? Resize
    {
        add => _driver.Resize += value;
        remove => _driver.Resize -= value;
    }

    public override event Action<TerminalSignalContext>? Signal
    {
        add => _driver.Signal += value;
        remove => _driver.Signal -= value;
    }

    public static SystemVirtualTerminal Instance { get; } = new();

    public override TerminalReader StdIn => _driver.StdIn;

    public override TerminalWriter StdOut => _driver.StdOut;

    public override TerminalWriter StdError => _driver.StdError;

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
        throw new TerminalException("This platform is not supported.");

    SystemVirtualTerminal()
    {
    }

    public override void GenerateSignal(TerminalSignal signal)
    {
        _driver.GenerateSignal(signal);
    }

    public override void EnableRawMode()
    {
        _driver.EnableRawMode();
    }

    public override void DisableRawMode()
    {
        _driver.DisableRawMode();
    }
}
