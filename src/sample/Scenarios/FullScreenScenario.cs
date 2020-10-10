using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Sample.Scenarios
{
    [SuppressMessage("Microsoft.Performance", "CA1812", Justification = "Used.")]
    sealed class FullScreenScenario : IScenario
    {
        public Task RunAsync()
        {
            using (_ = Terminal.AlternateScreen.Activate())
            {
                Terminal.OutLine("This text is rendered in the alternate screen buffer.");
                Terminal.OutLine();
                Terminal.OutLine("Press Enter to return to the main screen buffer.");

                _ = Terminal.ReadLine();
            }

            return Task.CompletedTask;
        }
    }
}
