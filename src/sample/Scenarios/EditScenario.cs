namespace Sample.Scenarios;

[SuppressMessage("Performance", "CA1812")]
sealed class EditScenario : IScenario
{
    public Task RunAsync()
    {
        var history = new MemoryTerminalHistory();
        var editor = new TerminalEditor(new(history));

        while (editor.ReadLine("edit> ") is string str)
            if (!string.IsNullOrWhiteSpace(str))
                Terminal.OutLine(str);

        return Task.CompletedTask;
    }
}
