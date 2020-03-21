using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Sample.Scenarios
{
    sealed class HostingScenario : IScenario
    {
        public Task RunAsync()
        {
            return TerminalHost.CreateDefaultBuilder().RunTerminalAsync();
        }
    }
}
