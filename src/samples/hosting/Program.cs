internal sealed class Program : IProgram
{
    public static async Task RunAsync(ProgramContext context)
    {
        context.ProcessExiting += () => OutLine("Exiting...");
        context.UnhandledException += e => OutLine($"Unhandled exception event: {e}");

        await OutLineAsync($"Arguments: {string.Join(' ', context.Arguments.ToArray())}");

        // Signals cannot be synthesized on Windows.
        if (!OperatingSystem.IsWindows())
        {
            await OutLineAsync("Generating a cancellation signal...");

            // Retrieve the token so that the event handler is wired up.
            var ct = context.CancellationToken;

            _ = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(5));

                GenerateSignal(TerminalSignal.Interrupt);
            });

            try
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, ct);
            }
            catch (TaskCanceledException)
            {
                await OutLineAsync("Caught a cancellation signal.");
            }
        }

        await OutLineAsync("Switching to raw mode...");

        try
        {
            EnableRawMode();
        }
        catch (TerminalNotAttachedException)
        {
            // Expected in CI.
        }
    }
}
