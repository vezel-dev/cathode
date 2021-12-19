sealed class Program : IProgram
{
    public static Task RunAsync(ProgramContext context)
    {
        context.ProcessExiting += () => Terminal.OutLine("Exiting...");
        context.UnhandledException += e => Terminal.OutLine($"Unhandled exception event: {e.GetType()}");

        Terminal.OutLine($"Arguments: {string.Join(' ', context.Arguments.ToArray())}");
        Terminal.OutLine("Switching to raw mode...");
        Terminal.EnableRawMode();

        context.ExitCode = 42;

        return Task.CompletedTask;
    }
}
