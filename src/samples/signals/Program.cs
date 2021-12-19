Terminal.OutLine("Available commands:");
Terminal.OutLine();
Terminal.OutLine("  install: Install signal handler.");
Terminal.OutLine("  uninstall: Uninstall signal handler.");
Terminal.OutLine("  close: Send close signal (SIGHUP / CTRL_CLOSE_EVENT).");
Terminal.OutLine("  interrupt: Send interrupt signal (SIGINT / CTRL_C_EVENT).");
Terminal.OutLine("  quit: Send quit signal (SIGQUIT / CTRL_BREAK_EVENT).");
Terminal.OutLine("  terminate: Send quit signal (SIGTERM / CTRL_SHUTDOWN_EVENT).");
Terminal.OutLine();

var run = true;

while (run)
{
    Terminal.Out("Command: ");

    static void OnSignal(TerminalSignalContext context)
    {
        context.Cancel = true;

        Terminal.OutLine($"Caught {context.Signal} signal.");
        Terminal.Out(Beep());
    }

    switch (Terminal.ReadLine())
    {
        case null:
            run = false;
            break;
        case "install":
            Terminal.Signaled += OnSignal;
            Terminal.OutLine("Installed signal handler.");
            break;
        case "uninstall":
            Terminal.Signaled -= OnSignal;
            Terminal.OutLine("Uninstalled signal handler.");
            break;
        case "close":
            Terminal.OutLine("Generating close signal.");
            Terminal.GenerateSignal(TerminalSignal.Close);
            break;
        case "interrupt":
            Terminal.OutLine("Generating interrupt signal.");
            Terminal.GenerateSignal(TerminalSignal.Interrupt);
            break;
        case "quit":
            Terminal.OutLine("Generating quit signal.");
            Terminal.GenerateSignal(TerminalSignal.Quit);
            break;
        case "terminate":
            Terminal.OutLine("Generating terminate signal.");
            Terminal.GenerateSignal(TerminalSignal.Terminate);
            break;
        case var cmd:
            Terminal.OutLine($"Unknown command '{cmd}'.");
            break;
    }
}
