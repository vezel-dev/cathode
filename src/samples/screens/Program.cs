await OutAsync(
    new ControlBuilder()
        .SetScreenBuffer(ScreenBuffer.Alternate)
        .MoveCursorTo(0, 0));

try
{
    await OutLineAsync("This text is rendered in the alternate screen buffer.");
    await OutLineAsync();
    await OutLineAsync("Returning to the main screen buffer...");

    await Task.Delay(TimeSpan.FromSeconds(10));
}
finally
{
    await OutAsync(SetScreenBuffer(ScreenBuffer.Main));
}
