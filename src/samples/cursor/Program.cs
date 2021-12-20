await OutLineAsync("Available commands:");
await OutLineAsync();
await OutLineAsync("  visible: Toggle cursor visibility.");
await OutLineAsync("  <style>: Set cursor style to the given style.");
await OutLineAsync();

await OutAsync(SetCursorVisibility(true));

var visible = true;
var run = true;

while (run)
{
    await OutAsync("Command: ");

    switch (await ReadLineAsync())
    {
        case null:
            run = false;
            break;
        case "visible":
            visible = !visible;

            await OutAsync(SetCursorVisibility(visible));
            await OutLineAsync($"Cursor is now {(visible ? "visible" : "invisible")}.");
            break;
        case var style when Enum.TryParse<CursorStyle>(style, true, out var s):
            await OutAsync(SetCursorStyle(s));
            await OutLineAsync($"Cursor style is now {s}.");
            break;
        case var cmd:
            await OutLineAsync($"Unknown command '{cmd}'.");
            break;
    }
}
