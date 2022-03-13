namespace Cathode.Hosting;

public interface IProgram
{
    static abstract Task RunAsync(ProgramContext context);
}
