using System;
using System.Threading.Tasks;

namespace Sample.Scenarios
{
    sealed class CursorScenario : IScenario
    {
        public Task RunAsync()
        {
            Terminal.OutLine("Available commands:");
            Terminal.OutLine();
            Terminal.OutLine("  visible: Toggle cursor visibility.");
            Terminal.OutLine("  blinking: Toggle cursor blinking.");
            Terminal.OutLine();

            while (true)
            {
                Terminal.Out("Command: ");

                switch (Terminal.ReadLine())
                {
                    case "visible":
                        var visible = Terminal.IsCursorVisible = !Terminal.IsCursorVisible;

                        Terminal.OutLine("Cursor is now {0}.", visible ? "visible" : "invisible");
                        break;
                    case "blinking":
                        var blinking = Terminal.IsCursorBlinking = !Terminal.IsCursorBlinking;

                        Terminal.OutLine("Cursor is now {0}.", blinking ? "blinking" : "static");
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
}
