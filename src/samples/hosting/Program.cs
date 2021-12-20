sealed class Program : IProgram
{
    public static async Task RunAsync(ProgramContext context)
    {
        context.ProcessExiting += () => OutLine("Exiting...");
        context.UnhandledException += e => OutLine($"Unhandled exception event: {e.GetType()}");

        await OutLineAsync($"Arguments: {string.Join(' ', context.Arguments.ToArray())}");
        await OutLineAsync("Switching to raw mode...");

        EnableRawMode();

        context.ExitCode = 42;
    }
}
