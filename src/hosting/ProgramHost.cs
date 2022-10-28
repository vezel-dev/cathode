namespace Vezel.Cathode.Hosting;

[EditorBrowsable(EditorBrowsableState.Never)]
[SuppressMessage("", "RS0030")]
public static class ProgramHost
{
    // This class is only meant to be used by generated code.

    private static int _running;

    public static async Task RunAsync<TProgram>(ReadOnlyMemory<string> arguments)
        where TProgram : IProgram
    {
        Check.Operation(Interlocked.Exchange(ref _running, 1) == 0);
        Check.ForEach(arguments.Span, arg => Check.Argument(arg != null, arguments));

        var context = new ProgramContext(arguments);
        var domain = AppDomain.CurrentDomain;

        domain.ProcessExit += (_, e) =>
        {
            context.RaiseProcessExit();

            // From this point on, invoking any terminal-related API will basically have undefined behavior. The only
            // way for that to happen is if a user has hooked into some of the framework events that we have banned.
            Terminal.DangerousRestoreSettings();

            Environment.ExitCode = context.ExitCode;
        };

        domain.UnhandledException += (_, e) =>
        {
            context.RaiseUnhandledException((Exception)e.ExceptionObject);

            // Most users expect ProcessExit to run on both normal and abnormal termination. This call makes that happen
            // and also ensures that the terminal cleanup we do in ProcessExit runs. One unfortunate downside of this
            // approach is that finally blocks will not run - see the comment in the catch block below.
            //
            // The choice of exit code 127 is just to match CoreCLR behavior. It has no special meaning. We set it on
            // the ProgramContext so that we overwrite any exit code set by the user prior to this point, but the user
            // may still overwrite it in a ProcessExit handler.
            Environment.Exit(context.ExitCode = 127);
        };

        try
        {
            await TProgram.RunAsync(context).ConfigureAwait(false);
        }
        catch (Exception)
        {
            // This seemingly pointless catching and throwing actually has a purpose: Since we call Environment.Exit in
            // the UnhandledException handler, any finally blocks on the call stack will not be invoked. We cannot do
            // anything about that on arbitrary threads, but here in the main thread we can at least catch the exception
            // such that UnhandledException is not raised (causing finally blocks to run properly), and then throw it
            // again to actually trigger UnhandledException since we know there are no finally blocks above us.
            throw;
        }

        // At this point, we will exit normally (raising ProcessExit) or abnormally (raising UnhandledException). Either
        // way, event handlers will run and terminal cleanup will happen.
    }
}
