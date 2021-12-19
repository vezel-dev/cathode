Terminal.OutLine("Entering raw mode.");
Terminal.OutLine();

Terminal.EnableRawMode();
Terminal.Out(SetMouseEvents(MouseEvents.All));

try
{
    for (var i = 0; i < 100; i++)
    {
        Terminal.Out($"0x{Terminal.ReadRaw():x2}");
        Terminal.Out("\r\n");
    }
}
finally
{
    Terminal.Out(SetMouseEvents(MouseEvents.None));
    Terminal.DisableRawMode();
}
