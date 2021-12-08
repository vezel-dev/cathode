Terminal.Out(
    new ControlBuilder()
        .SetScreenBuffer(ScreenBuffer.Alternate)
        .MoveCursorTo(0, 0));

try
{
    Terminal.OutLine("This text is rendered in the alternate screen buffer.");
    Terminal.OutLine();
    Terminal.OutLine("Press Enter to return to the main screen buffer.");

    _ = Terminal.ReadLine();
}
finally
{
    Terminal.Out(SetScreenBuffer(ScreenBuffer.Main));
}
