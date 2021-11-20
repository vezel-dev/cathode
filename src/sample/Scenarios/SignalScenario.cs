namespace Sample.Scenarios;

[SuppressMessage("Performance", "CA1812")]
sealed class SignalScenario : IScenario
{
    public Task RunAsync()
    {
        Terminal.OutLine("Available commands:");
        Terminal.OutLine();
        Terminal.OutLine("  install: Install break signal handler.");
        Terminal.OutLine("  uninstall: Uninstall break signal handler.");
        Terminal.OutLine("  interrupt: Send interrupt signal (SIGINT / CTRL_C_EVENT).");
        Terminal.OutLine("  quit: Send quit signal (SIGQUIT / CTRL_BREAK_EVENT).");
        Terminal.OutLine("  suspend: Send suspend signal (SIGTSTP / no-op on Windows).");
        Terminal.OutLine();

        while (true)
        {
            Terminal.Out("Command: ");

            static void OnBreakSignal(object? sender, TerminalBreakSignalEventArgs e)
            {
                e.Cancel = true;

                Terminal.OutLine("Caught {0} signal.", e.Signal);
                Terminal.Beep();
            }

            switch (Terminal.ReadLine())
            {
                case "install":
                    Terminal.BreakSignal += OnBreakSignal;
                    Terminal.OutLine("Installed break signal handler.");
                    break;
                case "uninstall":
                    Terminal.BreakSignal -= OnBreakSignal;
                    Terminal.OutLine("Uninstalled break signal handler.");
                    break;
                case "interrupt":
                    Terminal.OutLine("Generating interrupt signal.");
                    Terminal.GenerateBreakSignal(TerminalBreakSignal.Interrupt);
                    break;
                case "quit":
                    Terminal.OutLine("Generating quit signal.");
                    Terminal.GenerateBreakSignal(TerminalBreakSignal.Quit);
                    break;
                case "suspend":
                    Terminal.OutLine("Generating suspend signal.");
                    Terminal.GenerateSuspendSignal();
                    break;
                case null:
                    break;
                case var cmd:
                    Terminal.OutLine("Unknown command '{0}'.", cmd);
                    break;
            }
        }
    }
}
