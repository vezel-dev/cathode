await OutAsync(
    new ControlBuilder()
        .SetScreenBuffer(ScreenBuffer.Alternate)
        .MoveCursorTo(0, 0));

try
{
    await OutLineAsync("This text is rendered in the alternate screen buffer.");
    await OutLineAsync();
    await OutLineAsync("Press Enter to return to the main screen buffer.");

    _ = await ReadLineAsync();
}
finally
{
    await OutAsync(SetScreenBuffer(ScreenBuffer.Main));
}
