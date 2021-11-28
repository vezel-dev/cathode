using static System.Text.Control.ControlSequences;

namespace Sample.Scenarios;

[SuppressMessage("Performance", "CA1812")]
sealed class FullScreenScenario : Scenario
{
    public override Task RunAsync()
    {
        Terminal.Out(SetScreenBuffer(ScreenBuffer.Alternate));

        try
        {
            Terminal.OutLine("This text is rendered in the alternate screen buffer.");
            Terminal.OutLine();
            Terminal.OutLine("Press Enter to return to the main screen buffer.");

            _ = Terminal.ReadLine();
        }
        finally
        {
            Terminal.Out(SetScreenBuffer(ScreenBuffer.Main));
        }

        return Task.CompletedTask;
    }
}
