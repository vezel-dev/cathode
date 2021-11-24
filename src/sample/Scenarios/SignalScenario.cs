namespace Sample.Scenarios;

[SuppressMessage("Performance", "CA1812")]
sealed class SignalScenario : Scenario
{
    public override Task RunAsync()
    {
        Terminal.OutLine("Available commands:");
        Terminal.OutLine();
        Terminal.OutLine("  install: Install signal handler.");
        Terminal.OutLine("  uninstall: Uninstall signal handler.");
        Terminal.OutLine("  close: Send close signal (SIGHUP / CTRL_CLOSE_EVENT).");
        Terminal.OutLine("  interrupt: Send interrupt signal (SIGINT / CTRL_C_EVENT).");
        Terminal.OutLine("  quit: Send quit signal (SIGQUIT / CTRL_BREAK_EVENT).");
        Terminal.OutLine("  terminate: Send quit signal (SIGTERM / CTRL_SHUTDOWN_EVENT).");
        Terminal.OutLine();

        while (true)
        {
            Terminal.Out("Command: ");

            static void OnSignal(TerminalSignalContext context)
            {
                context.Cancel = true;

                Terminal.OutLine("Caught {0} signal.", context.Signal);
                Terminal.Beep();
            }

            switch (Terminal.ReadLine())
            {
                case "install":
                    Terminal.Signal += OnSignal;
                    Terminal.OutLine("Installed signal handler.");
                    break;
                case "uninstall":
                    Terminal.Signal -= OnSignal;
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
                case null:
                    break;
                case var cmd:
                    Terminal.OutLine("Unknown command '{0}'.", cmd);
                    break;
            }
        }
    }
}
