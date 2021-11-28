using static System.Text.Control.ControlSequences;

namespace Sample.Scenarios;

[SuppressMessage("Performance", "CA1812")]
sealed class RawScenario : Scenario
{
    public override Task RunAsync()
    {
        Terminal.OutLine("Entering raw mode.");
        Terminal.OutLine();

        Terminal.EnableRawMode();
        Terminal.Out(SetMouseEvents(MouseEvents.All));

        try
        {
            for (var i = 0; i < 100; i++)
            {
                Terminal.Out("0x{0:x2}", Terminal.ReadRaw());
                Terminal.Out("\r\n");
            }
        }
        finally
        {
            Terminal.DisableRawMode();
        }

        return Task.CompletedTask;
    }
}
