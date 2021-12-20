await OutLineAsync("Entering raw mode.");
await OutLineAsync();

EnableRawMode();

await OutAsync(SetMouseEvents(MouseEvents.All));

try
{
    for (var i = 0; i < 100; i++)
    {
        await OutAsync($"0x{await ReadRawAsync():x2}");
        await OutAsync("\r\n");
    }
}
finally
{
    await OutAsync(SetMouseEvents(MouseEvents.None));

    DisableRawMode();
}
