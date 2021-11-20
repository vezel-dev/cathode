namespace Sample;

static class Program
{
    static async Task<int> Main(string[] args)
    {
        Terminal.Title = nameof(Sample);

        static IEnumerable<Type> GetScenarios()
        {
            return Assembly.GetExecutingAssembly().DefinedTypes.Where(
                x => x.ImplementedInterfaces.Contains(typeof(IScenario))).OrderBy(x => x.Name);
        }

        if (args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
        {
            Terminal.ForegroundColor(255, 0, 0);
            Terminal.ErrorLine("Please supply a scenario name.");
            Terminal.ResetAttributes();

            Terminal.ErrorLine();
            Terminal.ErrorLine("Available scenarios:");
            Terminal.ErrorLine();

            foreach (var type in GetScenarios())
                Terminal.ErrorLine("  {0}", new string(type.Name.SkipLast("Scenario".Length).ToArray()));

            Terminal.ErrorLine();

            return 1;
        }

        var name = args[0].ToUpperInvariant();
        var t = GetScenarios().FirstOrDefault(x =>
            x.Name.StartsWith(name, StringComparison.InvariantCultureIgnoreCase));

        if (t == null)
        {
            Terminal.ForegroundColor(255, 0, 0);
            Terminal.ErrorLine("Could not find a scenario matching '{0}'.", args[0]);
            Terminal.ResetAttributes();

            return 1;
        }

        Terminal.ForegroundColor(0, 255, 0);
        Terminal.OutLine("Press Enter to run {0}.", t.Name);
        Terminal.ResetAttributes();

        _ = Terminal.ReadLine();

        Terminal.ClearScreen();
        Terminal.MoveCursorTo(0, 0);

        await ((IScenario)Activator.CreateInstance(t)!).RunAsync().ConfigureAwait(false);

        return 0;
    }
}
