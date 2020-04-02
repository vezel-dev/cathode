using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.Scenarios
{
    sealed class ResizeScenario : IScenario
    {
        public Task RunAsync()
        {
            Terminal.OutLine("Listening for resize events.");
            Terminal.OutLine();

            Terminal.Resize += (sender, e) =>
                Terminal.OutLine("Width = {0}, Height = {1}", e.Size.Width, e.Size.Height);

            return Task.Delay(Timeout.Infinite);
        }
    }
}
