await OutLineAsync("Entering raw mode.");
await OutLineAsync();

var array = new byte[1];

EnableRawMode();

await OutAsync(SetMouseEvents(MouseEvents.All));

try
{
    for (var i = 0; i < 100; i++)
    {
        if (await ReadAsync(array) == 0)
            break;

        await OutAsync($"0x{array[0]:x2}");
        await OutAsync("\r\n");
    }
}
finally
{
    await OutAsync(SetMouseEvents(MouseEvents.None));

    DisableRawMode();
}
