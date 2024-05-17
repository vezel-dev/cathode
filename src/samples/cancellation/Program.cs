// SPDX-License-Identifier: 0BSD

await OutAsync("Reading cooked input and cancelling after 5 seconds: ");

using var cts1 = new CancellationTokenSource(delay: TimeSpan.FromSeconds(5));

await OutLineAsync(await ReadLineAsync(cts1.Token));

await OutLineAsync("Entering raw mode and reading input. Then cancelling after 5 seconds.");
await OutLineAsync();

var array = new byte[1];

EnableRawMode();

try
{
    for (var i = 0; i < 2; i++)
    {
        await Task.Delay(TimeSpan.FromSeconds(5));

        using var cts2 = new CancellationTokenSource(delay: TimeSpan.FromSeconds(5));

        await OutAsync($"Round {i}...\r\n");

        while (true)
        {
            try
            {
                if (await ReadAsync(array, cts2.Token) == 0)
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
