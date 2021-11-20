using Microsoft.Extensions.Hosting;

namespace Sample.Scenarios;

[SuppressMessage("Performance", "CA1812")]
sealed class HostingScenario : IScenario
{
    public Task RunAsync()
    {
        return TerminalHost.CreateDefaultBuilder().RunTerminalAsync();
    }
}
