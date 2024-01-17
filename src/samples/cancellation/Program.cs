await OutAsync("Reading cooked input: ");
await OutLineAsync(await ReadLineAsync());

await OutLineAsync("Entering raw mode and reading input. Then canceling after 5 seconds.");
await OutLineAsync();

var array = new byte[1];

EnableRawMode();

try
{
    for (var i = 0; i < 2; i++)
    {
        await Task.Delay(TimeSpan.FromSeconds(5));

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        await OutAsync($"Round {i}...\r\n");

        while (true)
        {
            try
            {
                if (await ReadAsync(array, cts.Token) == 0)
                    break;
            }
            catch (OperationCanceledException)
            {
                await OutAsync("Canceled.\r\n");

                break;
            }

            await OutAsync($"0x{array[0]:x2}\r\n");
        }
    }
}
finally
{
    DisableRawMode();
}
