await OutAsync("Reading cooked input: ");
await OutLineAsync(await ReadLineAsync());

await OutLineAsync("Entering raw mode and reading input. Canceling after 5 seconds.");
await OutLineAsync();

EnableRawMode();

try
{
    using var cts = new CancellationTokenSource();

    cts.CancelAfter(TimeSpan.FromSeconds(5));

    while (true)
    {
        byte? b;

        try
        {
            b = await ReadRawAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            await OutAsync("Canceled.");
            await OutAsync("\r\n");

            break;
        }

        await OutAsync($"0x{b:x2}");
        await OutAsync("\r\n");
    }
}
finally
{
    DisableRawMode();
}
