namespace Sample.Scenarios;

[SuppressMessage("Performance", "CA1812")]
sealed class ResizeScenario : Scenario
{
    public override Task RunAsync()
    {
        Terminal.OutLine("Listening for resize events.");
        Terminal.OutLine();

        Terminal.Resize += size => Terminal.OutLine("Width = {0}, Height = {1}", size.Width, size.Height);

        return Task.Delay(Timeout.Infinite);
    }
}
