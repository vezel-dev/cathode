using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Sample.Scenarios
{
    [SuppressMessage("Microsoft.Performance", "CA1812", Justification = "Used.")]
    sealed class RawScenario : IScenario
    {
        public Task RunAsync()
        {
            Terminal.OutLine("Entering raw mode.");
            Terminal.OutLine();

            Terminal.SetRawMode(true, true);
            Terminal.SetMouseEvents(TerminalMouseEvents.All);

            try
            {
                for (var i = 0; i < 1000; i++)
                {
                    Terminal.Out("0x{0:x2}", Terminal.ReadRaw());
                    Terminal.Out("\r\n");
                }
            }
            finally
            {
                Terminal.SetRawMode(false, true);
            }

            return Task.CompletedTask;
        }
    }
}
