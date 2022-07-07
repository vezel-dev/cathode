namespace Vezel.Cathode.Hosting;

public interface IProgram
{
    public static abstract Task RunAsync(ProgramContext context);
}
