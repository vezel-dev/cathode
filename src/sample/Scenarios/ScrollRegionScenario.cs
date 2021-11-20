namespace Sample.Scenarios;

[SuppressMessage("Performance", "CA1812")]
sealed class ScrollRegionScenario : IScenario
{
    [SuppressMessage("Security", "CA5394")]
    public Task RunAsync()
    {
        Terminal.SetScrollRegion(2, 0);

        try
        {
            Terminal.Decorations(bold: true);
            Terminal.OutLine("The last string entered will be displayed here.");
            Terminal.ResetAttributes();
            Terminal.OutLine(new('-', Terminal.Size.Width));

            var rng = new Random();

            byte PickRandom()
            {
                return (byte)rng.Next(byte.MinValue, byte.MaxValue + 1);
            }

            while (true)
            {
                Terminal.Out("String: ");

                var str = Terminal.ReadLine() ?? string.Empty;

                Terminal.SaveCursorPosition();
                Terminal.MoveCursorTo(0, 0);
                Terminal.ClearLine();

                Terminal.ForegroundColor(PickRandom(), PickRandom(), PickRandom());
                Terminal.Out("Last string: {0}", str.ReplaceLineEndings(string.Empty));
                Terminal.ResetAttributes();

                Terminal.RestoreCursorPosition();
            }
        }
        finally
        {
            Terminal.SetScrollRegion(0, 0);
        }
    }
}
