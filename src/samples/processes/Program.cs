// SPDX-License-Identifier: 0BSD

await OutLineAsync("Launching 'bash'...");

using var cts = new CancellationTokenSource(delay: TimeSpan.FromSeconds(10));

var bash =
    new ChildProcessBuilder()
        .WithFileName("bash")
        .WithRedirections(false)
        .WithCancellationToken(cts.Token)
        .WithThrowOnError(false)
        .Run();

try
{
    await OutLineAsync($"'bash' exited with code: {await bash.Completion}");
}
catch (OperationCanceledException)
{
    await OutLineAsync();
    await OutLineAsync("Killed 'bash' due to timeout.");
}

await OutLineAsync("Entering raw mode and launching 'echo'...");

try
{
    EnableRawMode();
}
catch (TerminalNotAttachedException)
{
    // Expected in CI.
}

var echo = ChildProcess.Run("echo", "hello", "world");

try
{
    await OutLineAsync($"'echo' exited with code: {await echo.Completion}");
}
finally
{
    try
    {
        DisableRawMode();
    }
    catch (TerminalNotAttachedException)
    {
        // Expected in CI.
    }
}

await OutLineAsync($"Captured output: {(await echo.StandardOut.TextReader.ReadToEndAsync()).Trim()}");
