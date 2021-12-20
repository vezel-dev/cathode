await OutLineAsync("Available commands:");
await OutLineAsync();
await OutLineAsync("  install: Install signal handler.");
await OutLineAsync("  uninstall: Uninstall signal handler.");
await OutLineAsync("  close: Send close signal (SIGHUP / CTRL_CLOSE_EVENT).");
await OutLineAsync("  interrupt: Send interrupt signal (SIGINT / CTRL_C_EVENT).");
await OutLineAsync("  quit: Send quit signal (SIGQUIT / CTRL_BREAK_EVENT).");
await OutLineAsync("  terminate: Send quit signal (SIGTERM / CTRL_SHUTDOWN_EVENT).");
await OutLineAsync();

var run = true;

while (run)
{
    await OutAsync("Command: ");

    Resumed += () =>
    {
        OutLine("Caught SIGCONT signal.");
        Out(Beep());
    };

    static void OnSignal(TerminalSignalContext context)
    {
        context.Cancel = true;

        OutLine($"Caught {context.Signal} signal.");
        Out(Beep());
    }

    switch (await ReadLineAsync())
    {
        case null:
            run = false;
            break;
        case "install":
            Signaled += OnSignal;

            await OutLineAsync("Installed signal handler.");
            break;
        case "uninstall":
            Signaled -= OnSignal;

            await OutLineAsync("Uninstalled signal handler.");
            break;
        case "close":
            await OutLineAsync("Generating close signal.");

            GenerateSignal(TerminalSignal.Close);
            break;
        case "interrupt":
            await OutLineAsync("Generating interrupt signal.");

            GenerateSignal(TerminalSignal.Interrupt);
            break;
        case "quit":
            await OutLineAsync("Generating quit signal.");

            GenerateSignal(TerminalSignal.Quit);
            break;
        case "terminate":
            await OutLineAsync("Generating terminate signal.");

            GenerateSignal(TerminalSignal.Terminate);
            break;
        case var cmd:
            await OutLineAsync($"Unknown command '{cmd}'.");
            break;
    }
}
