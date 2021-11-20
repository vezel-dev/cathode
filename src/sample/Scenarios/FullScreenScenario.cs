namespace Sample.Scenarios;

[SuppressMessage("Performance", "CA1812")]
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
