using System;
using System.Threading;

namespace Sample
{
    static class Program
    {
        static void Main(string[] args)
        {
            Terminal.Title = nameof(Sample);

            Terminal.Break += (sender, e) =>
            {
                Terminal.OutLine("Received {0} event.", e.Key);

                e.Cancel = true;
            };

            Terminal.Clear();

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

            Terminal.OutForegroundColor(byte.MaxValue, byte.MinValue, byte.MinValue);
            Terminal.OutLine("This text is red.");
            Terminal.OutForegroundColor(byte.MinValue, byte.MaxValue, byte.MinValue);
            Terminal.OutLine("This text is green.");
            Terminal.OutForegroundColor(byte.MinValue, byte.MinValue, byte.MaxValue);
            Terminal.OutLine("This text is blue.");
            Terminal.OutResetAttributes();

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
            Terminal.OutLine("Exiting...");

            Thread.Sleep(1000);
        }
    }
}
