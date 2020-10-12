using System;
using System.Diagnostics.CodeAnalysis;
using System.Input;
using System.Threading.Tasks;

namespace Sample.Scenarios
{
    [SuppressMessage("Microsoft.Performance", "CA1812", Justification = "Used.")]
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
}
