Terminal.Out("Reading cooked input: ");
Terminal.OutLine(Terminal.ReadLine());

Terminal.OutLine("Entering raw mode and reading input. Canceling after 5 seconds.");
Terminal.OutLine();

Terminal.EnableRawMode();

try
{
    using var cts = new CancellationTokenSource();

    cts.CancelAfter(TimeSpan.FromSeconds(5));

    while (true)
    {
        byte? b;

        try
        {
            b = Terminal.ReadRaw(cts.Token);
        }
        catch (OperationCanceledException)
        {
            Terminal.Out("Canceled.");
            Terminal.Out("\r\n");

            break;
        }

        Terminal.Out($"0x{b:x2}");
        Terminal.Out("\r\n");
    }
}
finally
{
    Terminal.DisableRawMode();
}
