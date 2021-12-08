Terminal.OutLine("Available commands:");
Terminal.OutLine();
Terminal.OutLine("  visible: Toggle cursor visibility.");
Terminal.OutLine("  <style>: Set cursor style to the given style.");
Terminal.OutLine();

Terminal.Out(SetCursorVisibility(true));

var visible = true;

while (true)
{
    Terminal.Out("Command: ");

    switch (Terminal.ReadLine())
    {
        case "visible":
            visible = !visible;

            Terminal.Out(SetCursorVisibility(visible));
            Terminal.OutLine("Cursor is now {0}.", visible ? "visible" : "invisible");
            break;
        case null:
            break;
        case var style when Enum.TryParse<CursorStyle>(style, true, out var s):
            Terminal.Out(SetCursorStyle(s));
            Terminal.OutLine("Cursor style is now {0}.", s);
            break;
        case var cmd:
            Terminal.OutLine("Unknown command '{0}'.", cmd);
            break;
    }
}
