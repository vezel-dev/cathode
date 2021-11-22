using Microsoft.Extensions.Hosting;

namespace Sample.Scenarios;

[SuppressMessage("Performance", "CA1812")]
sealed class HostingScenario : Scenario
{
    public override Task RunAsync()
    {
        return TerminalHost.CreateDefaultBuilder().RunConsoleAsync();
    }
}
