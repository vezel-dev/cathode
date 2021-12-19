Terminal.OutLine("Launching 'bash'...");

var bash =
    new TerminalProcessBuilder()
        .WithFileName("bash")
        .Start();

await bash.WaitForExitAsync().ConfigureAwait(false);

Terminal.OutLine($"'bash' exited with code: {bash.ExitCode}");

var sb = new StringBuilder();

Terminal.OutLine("Entering raw mode and launching 'echo'...");
Terminal.EnableRawMode();

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

    await echo.WaitForExitAsync().ConfigureAwait(false);
}
finally
{
    Terminal.DisableRawMode();
}

Terminal.OutLine($"Captured output: {sb}");
