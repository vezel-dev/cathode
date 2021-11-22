namespace Sample.Scenarios;

[SuppressMessage("Performance", "CA1812")]
sealed class WidthScenario : Scenario
{
    public override Task RunAsync()
    {
        while (true)
        {
            Terminal.Out("String: ");

            var str = Terminal.ReadLine() ?? string.Empty;

            Terminal.OutLine("Width: {0}", TerminalWidth.Measure(str.ReplaceLineEndings(string.Empty)));
        }
    }
}
