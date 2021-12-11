sealed class Program : IProgram
{
    public static Task RunAsync(ProgramContext context)
    {
        context.ProcessExit += () => Terminal.OutLine("Exiting...");
        context.UnhandledException += e => Terminal.OutLine("Unhandled exception event: {0}", e.GetType());

        Terminal.OutLine("Arguments: {0}", string.Join(' ', context.Arguments.ToArray()));
        Terminal.OutLine("Switching to raw mode...");
        Terminal.EnableRawMode();

        context.ExitCode = 42;

        return Task.CompletedTask;
    }
}
