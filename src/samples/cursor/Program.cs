Terminal.OutLine("Available commands:");
Terminal.OutLine();
Terminal.OutLine("  visible: Toggle cursor visibility.");
Terminal.OutLine("  <style>: Set cursor style to the given style.");
Terminal.OutLine();

Terminal.Out(SetCursorVisibility(true));

var visible = true;
var run = true;

while (run)
{
    Terminal.Out("Command: ");

    switch (Terminal.ReadLine())
    {
        case null:
            run = false;
            break;
        case "visible":
            visible = !visible;

            Terminal.Out(SetCursorVisibility(visible));
            Terminal.OutLine($"Cursor is now {(visible ? "visible" : "invisible")}.");
            break;
        case var style when Enum.TryParse<CursorStyle>(style, true, out var s):
            Terminal.Out(SetCursorStyle(s));
            Terminal.OutLine($"Cursor style is now {s}.");
            break;
        case var cmd:
            Terminal.OutLine($"Unknown command '{cmd}'.");
            break;
    }
}
