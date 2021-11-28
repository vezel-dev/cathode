using static System.Text.Control.ControlSequences;

namespace Sample.Scenarios;

[SuppressMessage("Performance", "CA1812")]
sealed class ScrollRegionScenario : Scenario
{
    [SuppressMessage("Security", "CA5394")]
    public override Task RunAsync()
    {
        Terminal.Out(SetScrollMargin(2, Terminal.Size.Height));

        try
        {
            Terminal.Out(
                new ControlBuilder()
                    .SetDecorations(bold: true)
                    .PrintLine("The last string entered will be displayed here.")
                    .ResetAttributes());
            Terminal.OutLine(new('-', Terminal.Size.Width));

            var rng = new Random();

            byte PickRandom()
            {
                return (byte)rng.Next(byte.MinValue, byte.MaxValue + 1);
            }

            while (true)
            {
                Terminal.Out("Input: ");

                if (Terminal.ReadLine() is not string str)
                    break;

                Terminal.Out(
                    new ControlBuilder()
                        .SaveCursorState()
                        .MoveCursorTo(0, 0)
                        .ClearLine()
                        .SetForegroundColor(PickRandom(), PickRandom(), PickRandom())
                        .Print(str.ReplaceLineEndings(string.Empty))
                        .ResetAttributes()
                        .RestoreCursorState());
            }
        }
        finally
        {
            Terminal.Out(SetScrollMargin(0, Terminal.Size.Height));
        }

        return Task.CompletedTask;
    }
}
