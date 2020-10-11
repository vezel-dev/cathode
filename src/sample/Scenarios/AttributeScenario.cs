using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Sample.Scenarios
{
    [SuppressMessage("Microsoft.Performance", "CA1812", Justification = "Used.")]
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

            Terminal.Decorations(overline: true);
            Terminal.OutLine("This text is overlined.");
            Terminal.ResetAttributes();

            Terminal.Decorations(doubleUnderline: true);
            Terminal.OutLine("This text is doubly underlined.");
            Terminal.ResetAttributes();

            Terminal.OpenHyperlink(new("https://google.com"));
            Terminal.OutLine("This is a hyperlink.");
            Terminal.CloseHyperlink();

            Terminal.OpenHyperlink(new("https://google.com"), "google");
            Terminal.OutLine("This is a hyperlink with an ID.");
            Terminal.CloseHyperlink();

            return Task.CompletedTask;
        }
    }
}
