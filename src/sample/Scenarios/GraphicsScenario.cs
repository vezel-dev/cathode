using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Sample.Scenarios
{
    [SuppressMessage("Microsoft.Performance", "CA1812", Justification = "Used.")]
    sealed class GraphicsScenario : IScenario
    {
        public Task RunAsync()
        {
            Terminal.IsGraphicsEnabled = true;

            try
            {
                const string Message = "This text is enclosed in lines drawn with DEC Special Graphics.";

                Terminal.Out("l");

                for (var i = 0; i < Message.Length; i++)
                    Terminal.Out("q");

                Terminal.OutLine("k");
                Terminal.Out("x");

                Terminal.IsGraphicsEnabled = false;
                Terminal.Out(Message);
                Terminal.IsGraphicsEnabled = true;

                Terminal.OutLine("x");
                Terminal.Out("m");

                for (var i = 0; i < Message.Length; i++)
                    Terminal.Out("q");

                Terminal.OutLine("j");
            }
            finally
            {
                Terminal.IsGraphicsEnabled = false;
            }

            return Task.CompletedTask;
        }
    }
}
