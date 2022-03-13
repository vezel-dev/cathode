using Cathode.Extensions.Hosting;
using Microsoft.Extensions.Hosting;

using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

await TerminalHost.CreateDefaultBuilder().RunConsoleAsync(cts.Token);
