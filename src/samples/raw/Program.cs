Terminal.OutLine("Entering raw mode.");
Terminal.OutLine();

Terminal.EnableRawMode();
Terminal.Out(SetMouseEvents(MouseEvents.All));

try
{
    for (var i = 0; i < 100; i++)
    {
        Terminal.Out("0x{0:x2}", Terminal.ReadRaw());
        Terminal.Out("\r\n");
    }
}
finally
{
    Terminal.Out(SetMouseEvents(MouseEvents.None));
    Terminal.DisableRawMode();
}
