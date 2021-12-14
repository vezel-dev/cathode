Terminal.OutLine("Entering raw mode...");
Terminal.EnableRawMode();

try
{
    Terminal.OutLine("Launching Bash...");

    using var proc = Process.Start("bash");

    await proc.WaitForExitAsync().ConfigureAwait(false);

    Terminal.OutLine("Bash exited with code {0}.", proc.ExitCode);
    Terminal.Out("Reading a byte...");
    Terminal.OutLine("Got: 0x{0:x2}", Terminal.ReadRaw());
}
finally
{
    Terminal.DisableRawMode();
}
