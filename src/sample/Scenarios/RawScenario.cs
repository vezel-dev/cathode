namespace Sample.Scenarios;

[SuppressMessage("Performance", "CA1812")]
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
            for (var i = 0; i < 100; i++)
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
