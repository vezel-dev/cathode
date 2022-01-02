await OutLineAsync("Launching 'bash'...");

using var cts = new CancellationTokenSource();

cts.CancelAfter(TimeSpan.FromSeconds(10));

var bash =
    new TerminalProcessBuilder()
        .WithFileName("bash")
        .Start();

try
{
    await bash.WaitForExitAsync(cts.Token);
}
catch (OperationCanceledException)
{
    bash.Kill();

    await OutLineAsync();
    await OutLineAsync("Killed 'bash' due to timeout.");
}

await OutLineAsync($"'bash' exited with code: {bash.ExitCode}");

var sb = new StringBuilder();

await OutLineAsync("Entering raw mode and launching 'echo'...");

EnableRawMode();

try
{
    var echo =
        new TerminalProcessBuilder()
            .WithFileName("echo")
            .WithArguments("hello", "world")
            .WithRedirections(true)
            .Start();

    echo.StandardOutReceived += str =>
    {
        lock (sb)
            _ = sb.Append(str);
    };

    echo.StartReadingStandardOut();

    await echo.WaitForExitAsync();
}
finally
{
    DisableRawMode();
}

await OutLineAsync($"Captured output: {sb}");
