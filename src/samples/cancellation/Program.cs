await OutAsync("Reading cooked input: ");
await OutLineAsync(await ReadLineAsync());

await OutLineAsync("Entering raw mode and reading input. Canceling after 5 seconds.");
await OutLineAsync();

using var cts = new CancellationTokenSource();

cts.CancelAfter(TimeSpan.FromSeconds(5));

var array = new byte[1];

EnableRawMode();

try
{
    while (true)
    {
        try
        {
            if (await ReadAsync(array, cts.Token) == 0)
                break;
        }
        catch (OperationCanceledException)
        {
            await OutAsync("Canceled.");
            await OutAsync("\r\n");

            break;
        }

        await OutAsync($"0x{array[0]:x2}");
        await OutAsync("\r\n");
    }
}
finally
{
    DisableRawMode();
}
