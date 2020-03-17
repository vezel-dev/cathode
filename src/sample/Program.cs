using System;
using System.Threading;

namespace Sample
{
    static class Program
    {
        static void Main(string[] args)
        {
            Terminal.Title = nameof(Sample);

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

            Terminal.OutLine();
            Terminal.OutLine("Width: {0}", Terminal.Width);
            Terminal.OutLine("Height: {0}", Terminal.Height);
            Terminal.OutLine();

            for (var i = 0; i < 2; i++)
            {
                Terminal.Out("Reading: ");
                Terminal.OutLine("Result: {0}", Terminal.ReadLine());
                Terminal.OutLine();
            }

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
