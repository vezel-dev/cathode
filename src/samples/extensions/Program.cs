using Microsoft.Extensions.Hosting;

using var cts = new CancellationTokenSource();

cts.CancelAfter(TimeSpan.FromSeconds(10));

await TerminalHost.CreateDefaultBuilder().RunConsoleAsync(cts.Token);
