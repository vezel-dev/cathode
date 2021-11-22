using Sample.Scenarios;

namespace Sample;

static class Program
{
    static readonly IEnumerable<Scenario> _scenarios = new Scenario[]
    {
        new AttributeScenario(),
        new CursorScenario(),
        new EditScenario(),
        new FullScreenScenario(),
        new HostingScenario(),
        new RawScenario(),
        new ResizeScenario(),
        new ScrollRegionScenario(),
        new SignalScenario(),
        new WidthScenario(),
    }.OrderBy(s => s.Name).ToArray();

    static async Task<int> Main(string[] args)
    {
        Terminal.Title = nameof(Sample);

        if (args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
        {
            Terminal.ForegroundColor(255, 0, 0);
            Terminal.ErrorLine("Please supply a scenario name.");
            Terminal.ResetAttributes();

            Terminal.ErrorLine();
            Terminal.ErrorLine("Available scenarios:");
            Terminal.ErrorLine();

            foreach (var scenario in _scenarios)
                Terminal.ErrorLine("  {0}", scenario.Name);

            Terminal.ErrorLine();

            return 1;
        }

        var match = _scenarios.FirstOrDefault(s => s.Name.StartsWith(args[0], StringComparison.OrdinalIgnoreCase));

        if (match == null)
        {
            Terminal.ForegroundColor(255, 0, 0);
            Terminal.ErrorLine("Could not find a scenario matching '{0}'.", args[0]);
            Terminal.ResetAttributes();

            return 1;
        }

        Terminal.ForegroundColor(0, 255, 0);
        Terminal.OutLine("Press Enter to run the {0} scenario.", match.Name);
        Terminal.ResetAttributes();

        _ = Terminal.ReadLine();

        Terminal.ClearScreen();
        Terminal.MoveCursorTo(0, 0);

        await match.RunAsync().ConfigureAwait(false);

        return 0;
    }
}
