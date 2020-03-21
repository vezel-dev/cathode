using System;
using System.Threading.Tasks;

namespace Sample.Scenarios
{
    sealed class AttributeScenario : IScenario
    {
        public Task RunAsync()
        {
            Terminal.ForegroundColor(255, 0, 0);
            Terminal.OutLine("This text is red.");
            Terminal.ResetAttributes();

            Terminal.ForegroundColor(0, 255, 0);
            Terminal.OutLine("This text is green.");
            Terminal.ResetAttributes();

            Terminal.ForegroundColor(0, 0, 255);
            Terminal.OutLine("This text is blue.");
            Terminal.ResetAttributes();

            Terminal.Decorations(bold: true);
            Terminal.OutLine("This text is bold.");
            Terminal.ResetAttributes();

            Terminal.Decorations(faint: true);
            Terminal.OutLine("This text is faint.");
            Terminal.ResetAttributes();

            Terminal.Decorations(italic: true);
            Terminal.OutLine("This text is in italics.");
            Terminal.ResetAttributes();

            Terminal.Decorations(underline: true);
            Terminal.OutLine("This text is underlined.");
            Terminal.ResetAttributes();

            Terminal.Decorations(blink: true);
            Terminal.OutLine("This text is blinking.");
            Terminal.ResetAttributes();

            Terminal.Decorations(invert: true);
            Terminal.OutLine("This text is inverted.");
            Terminal.ResetAttributes();

            Terminal.Decorations(invisible: true);
            Terminal.OutLine("This text is invisible.");
            Terminal.ResetAttributes();

            Terminal.Decorations(strike: true);
            Terminal.OutLine("This text is struck through.");
            Terminal.ResetAttributes();

            return Task.CompletedTask;
        }
    }
}
