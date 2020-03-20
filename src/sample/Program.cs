using System;
using System.Threading;
using Microsoft.Extensions.Hosting;

namespace Sample
{
    static class Program
    {
        static void Main(string[] args)
        {
            static void OnBreakSignal(object? sender, TerminalBreakSignalEventArgs e)
            {
                Terminal.OutLine("Received {0} event.", e.Signal);

                e.Cancel = true;
            }

            Terminal.Title = nameof(Sample);

            Terminal.BreakSignal += OnBreakSignal;

            Terminal.ClearScreen();
            Terminal.MoveCursorTo(0, 0);

            Terminal.OutLine("Hello.");
            Terminal.OutLine();

            var stdin = Terminal.StdIn;
            var stdout = Terminal.StdOut;
            var stderr = Terminal.StdError;

            Terminal.OutLine("StdIn: IsRedirected = {0}, Encoding = '{1}'",
                stdin.IsRedirected, stdin.Encoding.EncodingName);
            Terminal.OutLine("StdOut: IsRedirected = {0}, Encoding = '{1}'",
                stdout.IsRedirected, stdout.Encoding.EncodingName);
            Terminal.OutLine("StdError: IsRedirected = {0}, Encoding = '{1}'",
                stderr.IsRedirected, stderr.Encoding.EncodingName);

            var (width, height) = Terminal.Size;

            Terminal.OutLine();
            Terminal.OutLine("Width: {0}", width);
            Terminal.OutLine("Height: {0}", height);
            Terminal.OutLine();

            Terminal.ForegroundColor(byte.MaxValue, byte.MinValue, byte.MinValue);
            Terminal.OutLine("This text is red.");
            Terminal.ForegroundColor(byte.MinValue, byte.MaxValue, byte.MinValue);
            Terminal.OutLine("This text is green.");
            Terminal.ForegroundColor(byte.MinValue, byte.MinValue, byte.MaxValue);
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

            for (var i = 0; i < 2; i++)
            {
                Terminal.OutLine();
                Terminal.Out("Reading: ");
                Terminal.OutLine("Result: {0}", Terminal.ReadLine());
            }

            Terminal.OutLine();
            Terminal.Out("Switching to raw mode: ");

            Terminal.SetRawMode(true, true);

            for (var i = 0; i < 10; i++)
                Terminal.Out("{0:x2} ", Terminal.ReadRaw());

            Terminal.SetRawMode(false, true);

            Terminal.OutLine();
            Terminal.OutLine();
            Terminal.OutLine("Starting host...");

            Terminal.BreakSignal -= OnBreakSignal;

            Thread.Sleep(2500);

            TerminalHost.CreateDefaultBuilder().RunTerminalAsync().GetAwaiter().GetResult();
        }
    }
}
